param(
    [string]$ProjectRoot = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
$tweaksPath = Join-Path $ProjectRoot 'Data\tweaks.json'
$operationsPath = Join-Path $ProjectRoot 'Data\registry_operations.json'

if (!(Test-Path $tweaksPath)) { throw "Arquivo não encontrado: $tweaksPath" }

$tweaks = Get-Content $tweaksPath -Raw -Encoding UTF8 | ConvertFrom-Json
$operations = @()
if (Test-Path $operationsPath) {
    $operations = @(Get-Content $operationsPath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Split-RegPath([string]$Path) {
    $p = $Path.IndexOf('\')
    if ($p -lt 0) { return @($Path, '') }
    return @($Path.Substring(0,$p), $Path.Substring($p+1))
}

function Get-Hive([string]$Root) {
    switch ($Root.ToUpperInvariant()) {
        'HKEY_LOCAL_MACHINE' { return [Microsoft.Win32.RegistryHive]::LocalMachine }
        'HKLM'               { return [Microsoft.Win32.RegistryHive]::LocalMachine }
        'HKEY_CURRENT_USER'  { return [Microsoft.Win32.RegistryHive]::CurrentUser }
        'HKCU'               { return [Microsoft.Win32.RegistryHive]::CurrentUser }
        'HKEY_CLASSES_ROOT'  { return [Microsoft.Win32.RegistryHive]::ClassesRoot }
        'HKCR'               { return [Microsoft.Win32.RegistryHive]::ClassesRoot }
        'HKEY_USERS'         { return [Microsoft.Win32.RegistryHive]::Users }
        'HKU'                { return [Microsoft.Win32.RegistryHive]::Users }
        'HKEY_CURRENT_CONFIG'{ return [Microsoft.Win32.RegistryHive]::CurrentConfig }
        'HKCC'               { return [Microsoft.Win32.RegistryHive]::CurrentConfig }
        default { throw "Hive não suportada: $Root" }
    }
}

function Parse-Expected($Tweak) {
    switch ($Tweak.RegistryType) {
        'DWord' {
            $hex = [string]$Tweak.ApplyRaw
            $u = [Convert]::ToUInt32($hex.Substring(6), 16)
            $bytes = [BitConverter]::GetBytes([uint32]$u)
            return @{ Kind=[Microsoft.Win32.RegistryValueKind]::DWord; Value=[BitConverter]::ToInt32($bytes,0) }
        }
        'String' {
            $raw = [string]$Tweak.ApplyRaw
            if ($raw.Length -ge 2 -and $raw[0] -eq '"' -and $raw[$raw.Length-1] -eq '"') {
                $raw = $raw.Substring(1,$raw.Length-2)
            }
            $raw = $raw.Replace('\\','\').Replace('\"','"')
            return @{ Kind=[Microsoft.Win32.RegistryValueKind]::String; Value=$raw }
        }
        'Binary' {
            $raw = [string]$Tweak.ApplyRaw
            $pos = $raw.IndexOf(':')
            if ($pos -ge 0) { $raw = $raw.Substring($pos+1) }
            [byte[]]$bytes = @($raw.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { [Convert]::ToByte($_.Trim(),16) })
            return @{ Kind=[Microsoft.Win32.RegistryValueKind]::Binary; Value=$bytes }
        }
        default { throw "Tipo não suportado no auditor: $($Tweak.RegistryType)" }
    }
}

$results = [System.Collections.Generic.List[object]]::new()
foreach ($t in $tweaks) {
    try {
        $parts = Split-RegPath ([string]$t.Path)
        $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey((Get-Hive $parts[0]), [Microsoft.Win32.RegistryView]::Default)
        try {
            $key = $base.OpenSubKey($parts[1], $false)
            if ($null -eq $key) {
                $results.Add([pscustomobject]@{Id=$t.Id; Tipo='Valor'; Status='CHAVE_AUSENTE'; Caminho=$t.Path; Nome=$t.Name; Esperado=$t.ApplyRaw})
                continue
            }
            try {
                $name = if ($t.Name -eq '@') { '' } else { [string]$t.Name }
                $names = @($key.GetValueNames())
                if (-not ($names -contains $name)) {
                    $results.Add([pscustomobject]@{Id=$t.Id; Tipo='Valor'; Status='VALOR_AUSENTE'; Caminho=$t.Path; Nome=$t.Name; Esperado=$t.ApplyRaw})
                    continue
                }
                $exp = Parse-Expected $t
                $kind = $key.GetValueKind($name)
                $actual = $key.GetValue($name, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
                $okKind = $kind -eq $exp.Kind
                $okValue = $false
                if ($exp.Kind -eq [Microsoft.Win32.RegistryValueKind]::Binary) {
                    $okValue = [Convert]::ToBase64String([byte[]]$actual) -eq [Convert]::ToBase64String([byte[]]$exp.Value)
                } else {
                    $okValue = $actual -eq $exp.Value
                }
                $status = if ($okKind -and $okValue) { 'OK' } elseif (-not $okKind) { 'TIPO_DIFERENTE' } else { 'VALOR_DIFERENTE' }
                $results.Add([pscustomobject]@{Id=$t.Id; Tipo='Valor'; Status=$status; Caminho=$t.Path; Nome=$t.Name; Esperado=$t.ApplyRaw})
            } finally { $key.Dispose() }
        } finally { $base.Dispose() }
    } catch {
        $results.Add([pscustomobject]@{Id=$t.Id; Tipo='Valor'; Status='ERRO'; Caminho=$t.Path; Nome=$t.Name; Esperado=$_.Exception.Message})
    }
}

foreach ($op in $operations) {
    try {
        $parts = Split-RegPath ([string]$op.Path)
        $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey((Get-Hive $parts[0]), [Microsoft.Win32.RegistryView]::Default)
        try {
            $key = $base.OpenSubKey($parts[1], $false)
            $exists = $null -ne $key
            if ($key) { $key.Dispose() }
            $status = switch ($op.OperationType) {
                'CreateKey' { if ($exists) { 'OK' } else { 'CHAVE_AUSENTE' } }
                'DeleteKey' { if (-not $exists) { 'OK' } else { 'AINDA_EXISTE' } }
                default { 'TIPO_OPERACAO_DESCONHECIDO' }
            }
            $expectedOp = if ($op.OperationType -eq 'CreateKey') { "[$($op.Path)]" } else { "[-$($op.Path)]" }
            $results.Add([pscustomobject]@{Id=$op.Id; Tipo=$op.OperationType; Status=$status; Caminho=$op.Path; Nome=''; Esperado=$expectedOp})
        } finally { $base.Dispose() }
    } catch {
        $results.Add([pscustomobject]@{Id=$op.Id; Tipo=$op.OperationType; Status='ERRO'; Caminho=$op.Path; Nome=''; Esperado=$_.Exception.Message})
    }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$out = Join-Path $ProjectRoot "auditoria-registro-$stamp.csv"
$results | Export-Csv -Path $out -NoTypeInformation -Encoding UTF8

Write-Host ''
Write-Host '=== AUDITORIA REGOPTIMIZER ===' -ForegroundColor Cyan
$results | Group-Object Status | Sort-Object Name | ForEach-Object {
    Write-Host ("{0,-24} {1,4}" -f $_.Name, $_.Count)
}
Write-Host ''
Write-Host "Total verificado: $($results.Count)" -ForegroundColor White
Write-Host "Relatório: $out" -ForegroundColor Yellow
Write-Host ''
Write-Host 'Observação: OK significa que o estado atual do Registro coincide com o valor/operação desejado no catálogo.' -ForegroundColor DarkGray
