#!/usr/bin/env bash

set -Eeuo pipefail

KIND_NODE_CONTAINER="tasksystem-dev-control-plane"

for required_command in docker pgrep; do
  if ! command -v "${required_command}" >/dev/null 2>&1; then
    echo "Required command is missing: ${required_command}" >&2
    exit 1
  fi
done

echo "[1/3] Stopping cloud-provider-kind"

if pgrep -f '^/usr/local/bin/cloud-provider-kind( |$)' >/dev/null 2>&1; then
  sudo pkill -f '^/usr/local/bin/cloud-provider-kind( |$)' || true

  for _ in $(seq 1 15); do
    if ! pgrep -f '^/usr/local/bin/cloud-provider-kind( |$)' >/dev/null 2>&1; then
      break
    fi

    sleep 1
  done

  echo "cloud-provider-kind stopped."
else
  echo "cloud-provider-kind is not running."
fi

echo
echo "[2/3] Stopping Gateway containers"

gateway_containers="$(
  docker ps \
    --filter 'name=kindccm-gw-' \
    --format '{{.Names}}'
)"

if [[ -n "${gateway_containers}" ]]; then
  while IFS= read -r container; do
    [[ -z "${container}" ]] && continue

    echo "Stopping ${container}..."
    docker stop "${container}" >/dev/null
  done <<< "${gateway_containers}"
else
  echo "No running Gateway containers found."
fi

echo
echo "[3/3] Stopping kind control-plane"

if docker container inspect "${KIND_NODE_CONTAINER}" >/dev/null 2>&1; then
  node_state="$(
    docker container inspect \
      --format '{{.State.Status}}' \
      "${KIND_NODE_CONTAINER}"
  )"

  if [[ "${node_state}" == "running" ]]; then
    docker stop "${KIND_NODE_CONTAINER}" >/dev/null
    echo "Control-plane container stopped."
  else
    echo "Control-plane container is already ${node_state}."
  fi
else
  echo "Control-plane container does not exist: ${KIND_NODE_CONTAINER}" >&2
  exit 1
fi

echo
echo "Local Kubernetes cluster stopped."
echo "Cluster containers and persistent data were not deleted."
