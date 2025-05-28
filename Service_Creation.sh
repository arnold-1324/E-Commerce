#!/usr/bin/env bash
set -euo pipefail

# List all your services here (matching docker-compose.yml)
services=(
  # api-gateway
  cart-service
  order-service
  payment-service
  user-service
  auth-service
  product-service
  wishlist-service
  recommendation-service
  notification-service
)

for svc in "${services[@]}"; do
  echo "⏳ Scaffolding $svc..."
  # Create service folder if it doesn't exist
  mkdir -p "$svc"
  pushd "$svc" >/dev/null

    # Remove any existing scaffold (optional)
    rm -rf "$svc-TempWebApi"

    # Scaffold directly into the service folder
    dotnet new webapi \
      --name "$svc" \
      --framework net8.0 \
      --no-openapi \
      --output . \
      --force

    # (No flattening needed since we output directly)

  popd >/dev/null
  echo "✅ $svc ready."
done

echo "🎉 All services scaffolded!"
