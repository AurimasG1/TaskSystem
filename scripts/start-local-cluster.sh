#!/usr/bin/env bash

set -Eeuo pipefail

KUBE_CONTEXT="kind-tasksystem-dev"
KIND_NODE_CONTAINER="tasksystem-dev-control-plane"
NAMESPACE="tasksystem"
GATEWAY_NAME="tasksystem-gateway"
CLOUD_PROVIDER_LOG="/tmp/tasksystem-cloud-provider-kind.log"

for required_command in docker kubectl cloud-provider-kind; do
  if ! command -v "${required_command}" >/dev/null 2>&1; then
    echo "Required command is missing: ${required_command}" >&2
    exit 1
  fi
done

echo "[1/5] Checking Docker"

if ! docker info >/dev/null 2>&1; then
  echo "Docker Engine is not reachable." >&2
  echo "Start Docker Desktop in Windows and run this script again." >&2
  exit 1
fi

echo "Docker is ready."

echo
echo "[2/5] Starting kind control-plane"

if ! kubectl config get-contexts "${KUBE_CONTEXT}" >/dev/null 2>&1; then
  echo "Kubernetes context does not exist: ${KUBE_CONTEXT}" >&2
  exit 1
fi

kubectl config use-context "${KUBE_CONTEXT}" >/dev/null

if ! docker container inspect "${KIND_NODE_CONTAINER}" >/dev/null 2>&1; then
  echo "Kind node container does not exist:" >&2
  echo "  ${KIND_NODE_CONTAINER}" >&2
  exit 1
fi

node_state="$(
  docker container inspect \
    --format '{{.State.Status}}' \
    "${KIND_NODE_CONTAINER}"
)"

case "${node_state}" in
  running)
    echo "Control-plane container is already running."
    ;;

  exited | created)
    echo "Starting control-plane container..."
    docker start "${KIND_NODE_CONTAINER}" >/dev/null
    ;;

  paused)
    echo "Unpausing control-plane container..."
    docker unpause "${KIND_NODE_CONTAINER}" >/dev/null
    ;;

  *)
    echo "Unsupported control-plane state: ${node_state}" >&2
    exit 1
    ;;
esac

echo
echo "[3/5] Waiting for Kubernetes API"

cluster_ready=false

for _ in $(seq 1 60); do
  if kubectl get --raw='/readyz' >/dev/null 2>&1; then
    cluster_ready=true
    break
  fi

  sleep 2
done

if [[ "${cluster_ready}" != "true" ]]; then
  echo "Kubernetes API did not become ready within 120 seconds." >&2
  exit 1
fi

echo "Kubernetes API is ready."

echo
echo "[4/5] Starting cloud-provider-kind"

if pgrep -f '^/usr/local/bin/cloud-provider-kind( |$)' >/dev/null 2>&1; then
  echo "cloud-provider-kind is already running."
else
  echo "Starting cloud-provider-kind..."
  echo "Sudo authentication may be required."

  sudo -v

  sudo sh -c \
    "KUBECONFIG='${HOME}/.kube/config' \
    nohup /usr/local/bin/cloud-provider-kind \
      --gateway-channel standard \
      --enable-lb-port-mapping \
      >'${CLOUD_PROVIDER_LOG}' 2>&1 &"

  provider_ready=false

  for _ in $(seq 1 30); do
    if pgrep -f '^/usr/local/bin/cloud-provider-kind( |$)' >/dev/null 2>&1; then
      provider_ready=true
      break
    fi

    sleep 1
  done

  if [[ "${provider_ready}" != "true" ]]; then
    echo "cloud-provider-kind did not start." >&2
    echo "Log: ${CLOUD_PROVIDER_LOG}" >&2
    exit 1
  fi
fi

kubectl wait \
  --for=condition=Accepted \
  gatewayclass/cloud-provider-kind \
  --timeout=120s

echo
echo "[5/5] Checking Gateway"

if kubectl get gateway "${GATEWAY_NAME}" \
  -n "${NAMESPACE}" >/dev/null 2>&1; then

  kubectl wait \
    --for=condition=Programmed \
    "gateway/${GATEWAY_NAME}" \
    -n "${NAMESPACE}" \
    --timeout=120s

  gateway_address="$(
    kubectl get gateway "${GATEWAY_NAME}" \
      -n "${NAMESPACE}" \
      -o jsonpath='{.status.addresses[0].value}'
  )"

  load_balancer_container=""

  for _ in $(seq 1 30); do
    load_balancer_container="$(
      docker ps \
        --filter 'name=kindccm-gw-' \
        --format '{{.Names}}' |
      head -n 1
    )"

    if [[ -n "${load_balancer_container}" ]]; then
      break
    fi

    sleep 2
  done

  echo "Gateway address: ${gateway_address}"

  if [[ -n "${load_balancer_container}" ]]; then
    host_port="$(
      docker port "${load_balancer_container}" 80/tcp |
      grep '^0.0.0.0:' |
      head -n 1 |
      awk -F: '{print $NF}'
    )"

    echo "Gateway container: ${load_balancer_container}"

    if [[ -n "${host_port}" ]]; then
      echo "Gateway host port: ${host_port}"
      echo
      echo "Health test:"
      echo "curl -H 'Host: api.tasksystem.local' http://127.0.0.1:${host_port}/health"
    fi
  else
    echo "Gateway Envoy container was not found." >&2
    echo "Check log: ${CLOUD_PROVIDER_LOG}" >&2
  fi
else
  echo "Gateway is not deployed yet; skipping Gateway readiness check."
fi

echo
echo "Local Kubernetes cluster is ready."
