Write-Host "=== Trade Order Service Setup ===" -ForegroundColor Cyan

# Step 1 - Tear down everything: containers, orphans, and volumes
Write-Host "`nTearing down containers and volumes..." -ForegroundColor Yellow
docker compose down --remove-orphans -v
if ($LASTEXITCODE -ne 0) {
    Write-Host "docker compose down failed." -ForegroundColor Red
    exit 1
}

# Belt-and-suspenders: force-remove the volume if it still exists
$volumeName = "agentic-platform-integrator_postgres_data"
$volumeExists = docker volume ls --format "{{.Name}}" | Select-String -Quiet $volumeName
if ($volumeExists) {
    Write-Host "Force-removing leftover volume: $volumeName" -ForegroundColor Yellow
    # Find and kill any container still holding the volume
    $holdingContainers = docker ps -aq --filter "volume=$volumeName"
    if ($holdingContainers) {
        docker rm -f $holdingContainers | Out-Null
    }
    docker volume rm $volumeName | Out-Null
}

# Step 2 - Start Postgres and RabbitMQ, wait for both to be healthy
Write-Host "`nStarting Postgres and RabbitMQ..." -ForegroundColor Yellow
docker compose up -d postgres rabbitmq
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start infrastructure services." -ForegroundColor Red
    exit 1
}

Write-Host "Waiting for Postgres to be ready..."
$retries = 30
do {
    Start-Sleep -Seconds 1
    docker exec portfolio-postgres pg_isready -U admin -d tradeorders 2>&1 | Out-Null
    $retries--
} while ($LASTEXITCODE -ne 0 -and $retries -gt 0)

if ($LASTEXITCODE -ne 0) {
    Write-Host "Postgres did not become ready in time." -ForegroundColor Red
    docker compose logs postgres
    exit 1
}
Write-Host "Postgres is ready." -ForegroundColor Green

Write-Host "Waiting for RabbitMQ to be ready..."
$retries = 30
do {
    Start-Sleep -Seconds 2
    docker exec portfolio-rabbitmq rabbitmq-diagnostics ping 2>&1 | Out-Null
    $retries--
} while ($LASTEXITCODE -ne 0 -and $retries -gt 0)

if ($LASTEXITCODE -ne 0) {
    Write-Host "RabbitMQ did not become ready in time." -ForegroundColor Red
    docker compose logs rabbitmq
    exit 1
}
Write-Host "RabbitMQ is ready." -ForegroundColor Green

# Step 3 - Build and start all application services
Write-Host "`nBuilding and starting Auth, API, and Sync..." -ForegroundColor Yellow
docker compose up -d --build auth api sync
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start application services." -ForegroundColor Red
    docker compose logs auth api sync
    exit 1
}

# Step 4 - Wait and show API startup logs
Write-Host "`nWaiting for API to start (10s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10
docker compose logs api --tail 30

Write-Host "`nSetup complete." -ForegroundColor Cyan
Write-Host "  Swagger UI      : http://localhost:8080/swagger" -ForegroundColor Green
Write-Host "  Health          : http://localhost:8080/health/live" -ForegroundColor Green
Write-Host "  Auth (OIDC)     : http://localhost:5001" -ForegroundColor Green
Write-Host "  RabbitMQ Mgmt   : http://localhost:15672  (guest/guest)" -ForegroundColor Green
