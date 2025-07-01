# Dockerfile for user-service

# ----------- Build Stage -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY user-service.csproj ./
RUN dotnet restore user-service.csproj

COPY . ./
RUN dotnet publish user-service.csproj -c Release -o /app/publish

# ----------- Runtime Stage -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "user-service.dll"]
