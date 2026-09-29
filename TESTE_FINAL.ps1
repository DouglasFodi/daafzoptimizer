param(
    [switch]$NoRun
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "=== RegOptimizer - teste final ===" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet não encontrado. Instale o .NET 8 SDK."
}

$config = Get-Content ".\Data\auth.json" -Raw | ConvertFrom-Json
Write-Host "Endpoint: $($config.Endpoint)"
if ($config.Endpoint -ne 'https://regoptimizer.duckdns.org/auth.php') {
    throw "Endpoint inesperado em Data\auth.json."
}

if (Test-Path ".\AuthServer\secret.php") {
    Write-Warning "Existe AuthServer\secret.php localmente. Não envie esse arquivo ao GitHub."
}

$sessionUrl = $env:REGOPTIMIZER_AUTH_URL
$userUrl = [Environment]::GetEnvironmentVariable('REGOPTIMIZER_AUTH_URL', 'User')
if ($sessionUrl) { Write-Warning "REGOPTIMIZER_AUTH_URL da sessão está definida: $sessionUrl" }
if ($userUrl) { Write-Warning "REGOPTIMIZER_AUTH_URL do usuário está definida: $userUrl" }

Write-Host "Limpando..." -ForegroundColor Yellow
dotnet clean
if ($LASTEXITCODE -ne 0) { throw "dotnet clean falhou." }

Write-Host "Restaurando dependências..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "dotnet restore falhou." }

Write-Host "Compilando Release..." -ForegroundColor Yellow
dotnet build -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build falhou." }

Write-Host "BUILD OK." -ForegroundColor Green

if (-not $NoRun) {
    Write-Host "Iniciando RegOptimizer para o teste de login..." -ForegroundColor Green
    dotnet run -c Release --no-build
}
