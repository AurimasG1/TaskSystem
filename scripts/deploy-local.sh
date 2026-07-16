#!/usr/bin/env bash

set -Eeuo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.." &&
  pwd
)"

OVERLAY_DIR="${ROOT_DIR}/k8s/overlays/local"
NAMESPACE="tasksystem"
MIGRATION_JOB="tasksystem-migrate"

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

if ! kubectl cluster-info >/dev/null 2>&1; then
  echo "Kubernetes cluster is not reachable." >&2
  echo "Current context: $(kubectl config current-context 2>/dev/null || echo none)" >&2
  exit 1
fi

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
