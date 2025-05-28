# Dockerfile for payment-service

###################################
### 1) BUILD ###
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy & restore only the project file
COPY payment-service.csproj ./
RUN dotnet restore payment-service.csproj

# install curl so healthcheck will work
RUN apt-get update \
 && apt-get install -y curl --no-install-recommends \
 && rm -rf /var/lib/apt/lists/*

# copy everything else & publish
COPY . ./
RUN dotnet publish payment-service.csproj \
    -c Release \
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
ENTRYPOINT ["dotnet", "payment-service.dll"]
