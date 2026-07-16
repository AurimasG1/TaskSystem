#!/usr/bin/env bash

set -Eeuo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.." &&
  pwd
)"

OVERLAY_DIR="${ROOT_DIR}/k8s/overlays/local"
NAMESPACE="tasksystem"
MIGRATION_JOB="tasksystem-migrate"
KUBE_CONTEXT="kind-tasksystem-dev"
KIND_NODE_CONTAINER="tasksystem-dev-control-plane"

error_handler() {
  exit_code=$?

  echo
  echo "Deployment failed with exit code ${exit_code}." >&2
  echo "Current workload status:" >&2

  kubectl get deployment,statefulset,job,pod \
    -n "${NAMESPACE}" \
    2>/dev/null || true

  exit "${exit_code}"
}

trap error_handler ERR

for command in kubectl docker; do
  if ! command -v "${command}" >/dev/null 2>&1; then
    echo "Required command is missing: ${command}" >&2
    exit 1
  fi
done

if ! docker info >/dev/null 2>&1; then
  echo "Docker Engine is not reachable." >&2
  echo "Start Docker Desktop and run this script again." >&2
  exit 1
fi

current_context="$(
  kubectl config current-context 2>/dev/null ||
  true
)"

if [[ "${current_context}" != "${KUBE_CONTEXT}" ]]; then
  echo "Unexpected Kubernetes context." >&2
  echo "Expected: ${KUBE_CONTEXT}" >&2
  echo "Current:  ${current_context:-none}" >&2
  exit 1
fi

if ! docker container inspect "${KIND_NODE_CONTAINER}" >/dev/null 2>&1; then
  echo "Kind control-plane container does not exist:" >&2
  echo "  ${KIND_NODE_CONTAINER}" >&2
  echo "The local kind cluster may need to be recreated." >&2
  exit 1
fi

node_state="$(
  docker container inspect \
    --format '{{.State.Status}}' \
    "${KIND_NODE_CONTAINER}"
)"

case "${node_state}" in
  running)
    echo "Kind control-plane container is running."
    ;;

  exited | created)
    echo "Starting kind control-plane container..."
    docker start "${KIND_NODE_CONTAINER}" >/dev/null
    ;;

  paused)
    echo "Unpausing kind control-plane container..."
    docker unpause "${KIND_NODE_CONTAINER}" >/dev/null
    ;;

  restarting)
    echo "Kind control-plane container is restarting..."
    ;;

  *)
    echo "Unsupported control-plane container state: ${node_state}" >&2
    exit 1
    ;;
esac

echo "Waiting for Kubernetes API server..."

cluster_ready=false

for _ in $(seq 1 60); do
  if kubectl get --raw='/readyz' >/dev/null 2>&1; then
    cluster_ready=true
    break
  fi

  sleep 2
done

if [[ "${cluster_ready}" != "true" ]]; then
  echo "Kubernetes API server did not become ready within 120 seconds." >&2

  docker ps -a \
    --filter "name=^/${KIND_NODE_CONTAINER}$" \
    --format 'table {{.Names}}\t{{.Status}}' >&2

  exit 1
fi

echo "Kubernetes cluster is ready."

if [[ ! -f "${OVERLAY_DIR}/secrets.env" ]]; then
  echo "Missing local secrets file:" >&2
  echo "  ${OVERLAY_DIR}/secrets.env" >&2
  echo "Create it from secrets.env.example and add local values." >&2
  exit 1
fi

echo
echo "[1/7] Validating rendered manifests"
"${ROOT_DIR}/scripts/validate-k8s.sh"

echo
echo "[2/7] Running Kubernetes server-side dry run"
kubectl apply \
  -k "${OVERLAY_DIR}" \
  --dry-run=server \
  >/dev/null

echo "Server-side validation passed."

echo
echo "[3/7] Recreating migration Job"
kubectl delete job "${MIGRATION_JOB}" \
  -n "${NAMESPACE}" \
  --ignore-not-found

echo
echo "[4/7] Applying Kubernetes manifests"
kubectl apply -k "${OVERLAY_DIR}"

echo
echo "[5/7] Waiting for MySQL"
kubectl rollout status statefulset/mysql \
  -n "${NAMESPACE}" \
  --timeout=180s

echo
echo "[6/7] Waiting for database migrations and API"
kubectl wait \
  --for=condition=complete \
  "job/${MIGRATION_JOB}" \
  -n "${NAMESPACE}" \
  --timeout=180s

kubectl rollout status deployment/tasksystem-api \
  -n "${NAMESPACE}" \
  --timeout=180s

echo
echo "[7/7] Deployment summary"

kubectl get deployment,statefulset,job,pod,pvc \
  -n "${NAMESPACE}"

echo
kubectl get hpa,gateway,httproute,networkpolicy \
  -n "${NAMESPACE}"

echo
echo "Local deployment completed successfully."
