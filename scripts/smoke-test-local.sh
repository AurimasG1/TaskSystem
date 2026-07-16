#!/usr/bin/env bash

set -Eeuo pipefail

NAMESPACE="tasksystem"
GATEWAY_NAME="tasksystem-gateway"
ROUTE_HOST="api.tasksystem.local"

for required_command in kubectl docker curl; do
  if ! command -v "${required_command}" >/dev/null 2>&1; then
    echo "Required command is missing: ${required_command}" >&2
    exit 1
  fi
done

echo "[1/6] Checking Kubernetes API"

if ! kubectl get --raw='/readyz' >/dev/null 2>&1; then
  echo "Kubernetes API is not ready." >&2
  echo "Run ./scripts/start-local-cluster.sh first." >&2
  exit 1
fi

echo "Kubernetes API is ready."

echo
echo "[2/6] Checking workloads"

kubectl rollout status statefulset/mysql \
  -n "${NAMESPACE}" \
  --timeout=60s

kubectl rollout status deployment/tasksystem-api \
  -n "${NAMESPACE}" \
  --timeout=60s

kubectl wait \
  --for=condition=complete \
  job/tasksystem-migrate \
  -n "${NAMESPACE}" \
  --timeout=60s

echo
echo "[3/6] Checking Gateway API resources"

kubectl wait \
  --for=condition=Programmed \
  "gateway/${GATEWAY_NAME}" \
  -n "${NAMESPACE}" \
  --timeout=60s

route_conditions="$(
  kubectl get httproute tasksystem-api \
    -n "${NAMESPACE}" \
    -o jsonpath='{range .status.parents[*].conditions[*]}{.type}={.status}{"\n"}{end}'
)"

echo "${route_conditions}"

grep -q '^Accepted=True$' <<< "${route_conditions}"
grep -q '^ResolvedRefs=True$' <<< "${route_conditions}"

echo
echo "[4/6] Finding Gateway host port"

gateway_container="$(
  docker ps \
    --filter 'name=kindccm-gw-' \
    --format '{{.Names}}' |
  head -n 1
)"

if [[ -z "${gateway_container}" ]]; then
  echo "Gateway container was not found." >&2
  echo "Run ./scripts/start-local-cluster.sh first." >&2
  exit 1
fi

host_port="$(
  docker port "${gateway_container}" 80/tcp |
  grep '^0.0.0.0:' |
  head -n 1 |
  awk -F: '{print $NF}'
)"

if [[ -z "${host_port}" ]]; then
  echo "Gateway host port was not found." >&2
  exit 1
fi

echo "Gateway container: ${gateway_container}"
echo "Gateway host port: ${host_port}"

base_url="http://127.0.0.1:${host_port}"

echo
echo "[5/6] Testing public health endpoint"

health_status="$(
  curl \
    --silent \
    --show-error \
    --output /dev/null \
    --write-out '%{http_code}' \
    --connect-timeout 3 \
    --max-time 10 \
    -H "Host: ${ROUTE_HOST}" \
    "${base_url}/health"
)"

echo "/health returned HTTP ${health_status}"

if [[ "${health_status}" != "200" ]]; then
  echo "Expected HTTP 200 from /health." >&2
  exit 1
fi

echo
echo "[6/6] Testing protected endpoint"

protected_status="$(
  curl \
    --silent \
    --show-error \
    --output /dev/null \
    --write-out '%{http_code}' \
    --connect-timeout 3 \
    --max-time 10 \
    -H "Host: ${ROUTE_HOST}" \
    "${base_url}/api/user/me"
)"

echo "/api/user/me returned HTTP ${protected_status}"

if [[ "${protected_status}" != "401" ]]; then
  echo "Expected HTTP 401 from unauthenticated /api/user/me." >&2
  exit 1
fi

echo
echo "Local Kubernetes smoke test passed."
echo "Gateway URL: ${base_url}"
