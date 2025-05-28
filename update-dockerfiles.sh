#!/usr/bin/env bash
set -euo pipefail

services=(
  # search-service
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
  # Derive a PascalCase project name from folder name, e.g. search-service → SearchService
  projName=$(echo "$svc" | sed -r 's/(^|-)([a-z])/\U\2/g')
  echo "Patching $svc/Dockerfile to use $projName.csproj …"

  cat > "$svc/Dockerfile" <<EOF
# Dockerfile for $svc

###################################
### 1) BUILD ###
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy & restore only the project file
COPY ${projName}.csproj ./
RUN dotnet restore ${projName}.csproj

# install curl so healthcheck will work
RUN apt-get update \\
 && apt-get install -y curl --no-install-recommends \\
 && rm -rf /var/lib/apt/lists/*

# copy everything else & publish
COPY . ./
RUN dotnet publish ${projName}.csproj \\
    -c Release \\
    -o /app/publish

###################################
### 2) RUNTIME ###
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# pull in published output
COPY --from=build /app/publish ./

# expose & run
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "${projName}.dll"]
EOF

done

echo "✅ All Dockerfiles updated to use each service’s .csproj. Now run:"
echo "   docker-compose up --build -d"
