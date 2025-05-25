#!/usr/bin/env bash
set -euo pipefail

# List all your services here
services=(
  search-service
  inventory-service
  routing-service
  recommendation-service
  pricing-service
  analytics-service
  cache-service
  cart-service
  order-service
  auth-service
)

for svc in "${services[@]}"; do
  echo "⏳ Scaffolding $svc..."
  mkdir -p "$svc"
  pushd "$svc" >/dev/null

    # Clean up any existing TempWebApi so --force isn’t strictly necessary 
    rm -rf TempWebApi

    # scaffold targeting net8.0, no-openapi, and force overwrite
    dotnet new webapi \
      -o TempWebApi \
      --framework net8.0 \
      --no-openapi \
      --force

    # flatten
    shopt -s dotglob
    mv TempWebApi/* . 2>/dev/null || true
    mv TempWebApi/.[!.]* . 2>/dev/null || true
    shopt -u dotglob

    rm -rf TempWebApi

  popd >/dev/null
  echo "✅ $svc ready."
done

echo "🎉 All services scaffolded!"
