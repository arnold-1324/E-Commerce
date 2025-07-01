# Dockerfile for cart-service

# ----------- Build Stage -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY cart-service.csproj ./
RUN dotnet restore cart-service.csproj

RUN apt-get update \
 && apt-get install -y curl --no-install-recommends \
 && rm -rf /var/lib/apt/lists/*

COPY . ./
RUN dotnet publish cart-service.csproj -c Release -o /app/publish

# ----------- Runtime Stage -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "cart-service.dll"]
