# Dockerfile for routing-service

# Use the official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Use the official ASP.NET runtime image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app/out
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "TempWebApi.dll"]
