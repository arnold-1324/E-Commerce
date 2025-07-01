# Dockerfile for notification-service

# ----------- Build Stage -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY notification-service.csproj ./
RUN dotnet restore notification-service.csproj

COPY . ./
RUN dotnet publish notification-service.csproj -c Release -o /app/publish

# ----------- Runtime Stage -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "notification-service.dll"]
