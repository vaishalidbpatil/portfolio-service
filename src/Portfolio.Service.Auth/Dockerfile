# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Portfolio.Service.Auth/Portfolio.Service.Auth.csproj src/Portfolio.Service.Auth/
RUN dotnet restore src/Portfolio.Service.Auth/Portfolio.Service.Auth.csproj

COPY src/Portfolio.Service.Auth/ src/Portfolio.Service.Auth/
RUN dotnet publish src/Portfolio.Service.Auth/Portfolio.Service.Auth.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

# Copy before switching user so appuser owns /app — required for Duende's tempkey.jwk write
COPY --from=build --chown=appuser:appgroup /app/publish /app

WORKDIR /app
USER appuser

ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001

ENTRYPOINT ["dotnet", "Portfolio.Service.Auth.dll"]
