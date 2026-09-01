# ============================================================================
# SmartBuy - Deploy al stack Docker local.
# Reconstruye las imágenes con el código actual (BE del repo, FE de la carpeta
# hermana SmartBuy.UI) y reemplaza los contenedores que cambiaron. La base y
# su volumen nunca se tocan. Gracias al cache de capas, si solo cambió uno de
# los dos, el otro ni se recompila.
# ============================================================================
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host ""
Write-Host "=== Deploy de SmartBuy al stack Docker ===" -ForegroundColor Green
docker compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "El deploy fallo: revisa los errores de arriba." -ForegroundColor Red
    exit 1
}

Write-Host ""
docker compose ps
Write-Host ""
Write-Host "Listo. El stack quedo corriendo:" -ForegroundColor Green
Write-Host "  Front:   http://localhost:4300"
Write-Host "  Swagger: http://localhost:5100/swagger"
Write-Host "  Bots:    docker logs -f smartbuy-api"
