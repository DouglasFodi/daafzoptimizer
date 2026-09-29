$ErrorActionPreference = 'Stop'
$subKey = 'Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}\InprocServer32'
$regPath = 'HKCU\Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}\InprocServer32'

Write-Host "Verificando: $regPath" -ForegroundColor Cyan
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($subKey, $false)
if ($null -eq $key) {
    Write-Host 'ERRO: InprocServer32 não existe.' -ForegroundColor Red
    exit 1
}

try {
    $names = $key.GetValueNames()
    $hasDefault = $names -contains ''
    if (-not $hasDefault) {
        Write-Host 'ERRO: InprocServer32 existe, mas o valor padrão sem nome não foi criado.' -ForegroundColor Red
        Write-Host 'No formato .reg, isso significa que @="" ainda NÃO existe.' -ForegroundColor Yellow
        exit 2
    }

    $kind = $key.GetValueKind('')
    $value = $key.GetValue('', '__MISSING__', [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
    Write-Host "Valor padrão explícito: SIM" -ForegroundColor Green
    Write-Host "Tipo: $kind"
    Write-Host ("Conteúdo entre colchetes: [" + [string]$value + "]")

    if ($kind -ne [Microsoft.Win32.RegistryValueKind]::String) {
        Write-Host "ERRO: deveria ser REG_SZ, mas é $kind." -ForegroundColor Red
        exit 3
    }
    if ([string]$value -ne '') {
        Write-Host 'ERRO: o valor padrão não está vazio.' -ForegroundColor Red
        exit 4
    }

    Write-Host 'OK: existe exatamente o equivalente a @="".' -ForegroundColor Green
    Write-Host ''
    & reg.exe query $regPath /ve
    exit 0
}
finally {
    $key.Dispose()
}
