# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/Portfolio.Service.Models/Portfolio.Service.Models.csproj src/Portfolio.Service.Models/
COPY src/Portfolio.Service.Configuration/Portfolio.Service.Configuration.csproj src/Portfolio.Service.Configuration/
COPY src/Portfolio.Service.Database/Portfolio.Service.Database.csproj src/Portfolio.Service.Database/
COPY src/Portfolio.Service.DataAccess/Portfolio.Service.DataAccess.csproj src/Portfolio.Service.DataAccess/
COPY src/Portfolio.Service.ServiceInterfaces/Portfolio.Service.ServiceInterfaces.csproj src/Portfolio.Service.ServiceInterfaces/
COPY src/Portfolio.Service.Services/Portfolio.Service.Services.csproj src/Portfolio.Service.Services/
COPY src/Portfolio.Service.Api/Portfolio.Service.Api.csproj src/Portfolio.Service.Api/
RUN dotnet restore src/Portfolio.Service.Api/Portfolio.Service.Api.csproj

# Copy source and build
COPY src/ src/
RUN dotnet publish src/Portfolio.Service.Api/Portfolio.Service.Api.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
USER appuser

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=5s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Portfolio.Service.Api.dll"]
