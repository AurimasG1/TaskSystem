#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.." &&
  pwd
)"

OVERLAY_DIR="${ROOT_DIR}/k8s/overlays/local"
SECRETS_FILE="${OVERLAY_DIR}/secrets.env"
SECRETS_EXAMPLE="${OVERLAY_DIR}/secrets.env.example"
RENDERED_FILE="$(mktemp)"
TEMPORARY_SECRETS=false

cleanup() {
  rm -f "${RENDERED_FILE}"

  if [[ "${TEMPORARY_SECRETS}" == "true" ]]; then
    rm -f "${SECRETS_FILE}"
  fi
}

trap cleanup EXIT

if [[ ! -f "${SECRETS_FILE}" ]]; then
  if [[ ! -f "${SECRETS_EXAMPLE}" ]]; then
    echo "Missing ${SECRETS_EXAMPLE}" >&2
    exit 1
  fi

  cp "${SECRETS_EXAMPLE}" "${SECRETS_FILE}"
  TEMPORARY_SECRETS=true
fi

echo "Rendering Kustomize overlay..."

kubectl kustomize "${OVERLAY_DIR}" > "${RENDERED_FILE}"

if [[ ! -s "${RENDERED_FILE}" ]]; then
  echo "Kustomize produced an empty manifest." >&2
  exit 1
fi

echo "Validating Kubernetes schemas..."

docker run \
  --rm \
  -i \
  --entrypoint /kubeconform \
  ghcr.io/yannh/kubeconform:v0.8.0 \
  -strict \
  -summary \
  -kubernetes-version 1.36.1 \
  -skip Gateway,HTTPRoute \
  < "${RENDERED_FILE}"

echo "Kubernetes manifest validation completed successfully."
