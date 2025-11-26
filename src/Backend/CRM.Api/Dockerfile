# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["CRM.Api/CRM.Api.csproj", "CRM.Api/"]
COPY ["CRM.Application/CRM.Application.csproj", "CRM.Application/"]
COPY ["CRM.Domain/CRM.Domain.csproj", "CRM.Domain/"]
COPY ["CRM.Infrastructure/CRM.Infrastructure.csproj", "CRM.Infrastructure/"]

RUN dotnet restore "CRM.Api/CRM.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CRM.Api"
RUN dotnet build "CRM.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CRM.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CRM.Api.dll"]
