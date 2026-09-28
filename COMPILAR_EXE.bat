@echo off
setlocal
cd /d "%~dp0"

echo ========================================
echo RegOptimizer - Compilacao Windows x64
echo ========================================
where dotnet >nul 2>nul
if errorlevel 1 (
  echo.
  echo ERRO: .NET 8 SDK nao encontrado.
  echo Instale o .NET 8 SDK e execute novamente.
  pause
  exit /b 1
)

dotnet restore
if errorlevel 1 goto :erro

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 goto :erro

echo.
echo Concluido.
echo Executavel/pacote em: %CD%\publish
start "" "%CD%\publish"
pause
exit /b 0

:erro
echo.
echo Falha na compilacao. Veja as mensagens acima.
pause
exit /b 1
