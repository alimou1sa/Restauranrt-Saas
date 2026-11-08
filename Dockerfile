# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files first so NuGet restore is cached between builds.
COPY RestaurantSaas_Api/RestaurantSaas_Api.csproj RestaurantSaas_Api/
COPY RestaurantSaaS.Application/RestaurantSaaS.Application.csproj RestaurantSaaS.Application/
COPY RestaurantSaaS.Domain/RestaurantSaaS.Domain.csproj RestaurantSaaS.Domain/
COPY RestaurantSaaS.Infrastructure/RestaurantSaaS.Infrastructure.csproj RestaurantSaaS.Infrastructure/
RUN dotnet restore RestaurantSaas_Api/RestaurantSaas_Api.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish RestaurantSaas_Api/RestaurantSaas_Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080

COPY --from=build /app/publish .

# .NET 8 images ship a non-root user (uid 1654); run as it instead of root.
USER $APP_UID

ENTRYPOINT ["dotnet", "RestaurantSaas_Api.dll"]
