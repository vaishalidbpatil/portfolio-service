# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Portfolio.TradeOrderService.Auth/Portfolio.TradeOrderService.Auth.csproj src/Portfolio.TradeOrderService.Auth/
RUN dotnet restore src/Portfolio.TradeOrderService.Auth/Portfolio.TradeOrderService.Auth.csproj

COPY src/Portfolio.TradeOrderService.Auth/ src/Portfolio.TradeOrderService.Auth/
RUN dotnet publish src/Portfolio.TradeOrderService.Auth/Portfolio.TradeOrderService.Auth.csproj \
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

ENTRYPOINT ["dotnet", "Portfolio.TradeOrderService.Auth.dll"]
