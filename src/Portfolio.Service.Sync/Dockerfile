# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Portfolio.Service.Models/Portfolio.Service.Models.csproj src/Portfolio.Service.Models/
COPY src/Portfolio.Service.Configuration/Portfolio.Service.Configuration.csproj src/Portfolio.Service.Configuration/
COPY src/Portfolio.Service.Database/Portfolio.Service.Database.csproj src/Portfolio.Service.Database/
COPY src/Portfolio.Service.DataAccess/Portfolio.Service.DataAccess.csproj src/Portfolio.Service.DataAccess/
COPY src/Portfolio.Service.Sync/Portfolio.Service.Sync.csproj src/Portfolio.Service.Sync/
RUN dotnet restore src/Portfolio.Service.Sync/Portfolio.Service.Sync.csproj

COPY src/ src/
RUN dotnet publish src/Portfolio.Service.Sync/Portfolio.Service.Sync.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Portfolio.Service.Sync.dll"]
