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
)

for svc in "${services[@]}"; do
  echo "⏳ Scaffolding $svc..."
  # ensure the folder exists and cd into it
  mkdir -p "$svc"
  pushd "$svc" >/dev/null

    # generate into a temp subfolder so we can flatten
    dotnet new webapi -o TempWebApi

    # move everything up, overwrite existing if needed
    shopt -s dotglob
    mv TempWebApi/* .
    mv TempWebApi/.[!.]* . 2>/dev/null || true
    shopt -u dotglob

    # remove the empty scaffold folder
    rm -rf TempWebApi

  popd >/dev/null
  echo "✅ $svc ready."
done

echo "🎉 All services scaffolded!"