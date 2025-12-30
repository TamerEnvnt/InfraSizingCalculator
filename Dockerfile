# InfraSizing Calculator - Docker Build
# Multi-stage build for optimized image size

# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY InfraSizingCalculator.slnx .
COPY src/InfraSizingCalculator/InfraSizingCalculator.csproj src/InfraSizingCalculator/
COPY tests/InfraSizingCalculator.UnitTests/InfraSizingCalculator.UnitTests.csproj tests/InfraSizingCalculator.UnitTests/

# Restore dependencies (cached unless project files change)
RUN dotnet restore src/InfraSizingCalculator/InfraSizingCalculator.csproj

# Copy all source code
COPY src/ src/

# Build in Release mode
WORKDIR /src/src/InfraSizingCalculator
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish -c Release -o /app/publish --no-build

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r infrasizing && useradd -r -g infrasizing infrasizing

# Create directories for data and logs
RUN mkdir -p /app/data /app/logs && \
    chown -R infrasizing:infrasizing /app

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R infrasizing:infrasizing /app

# Switch to non-root user
USER infrasizing

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/infrasizing.db"
ENV ConnectionStrings__IdentityConnection="Data Source=/app/data/identity.db"
ENV Database__AutoMigrate=true

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "InfraSizingCalculator.dll"]
