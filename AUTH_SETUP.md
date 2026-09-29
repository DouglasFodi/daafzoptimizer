# Autenticação — configuração final

O RegOptimizer valida a senha por HTTPS antes de abrir a janela principal.
A senha e o hash **não ficam no aplicativo nem no GitHub**.

## Endpoint oficial

O cliente está configurado em `Data/auth.json` para:

```text
https://regoptimizer.duckdns.org/auth.php
```

A EC2 executa Nginx + PHP 8.3-FPM e mantém os arquivos de produção em:

```text
/var/www/html/regoptimizer/auth.php
/var/www/html/regoptimizer/secret.php
```

`secret.php` é exclusivo do servidor e está bloqueado pelo `.gitignore`.

## Trocar a senha

Gere um novo hash no servidor:

```bash
php -r 'echo password_hash("NOVA-SENHA", PASSWORD_DEFAULT), PHP_EOL;'
```

Depois substitua apenas `password_hash` em:

```text
/var/www/html/regoptimizer/secret.php
```

Não é necessário recompilar o aplicativo.

## Teste do endpoint no Windows PowerShell

```powershell
$body = @{
    password = "SUA_SENHA"
    application = "RegOptimizer"
    version = "3.8"
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://regoptimizer.duckdns.org/auth.php" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

Sucesso esperado:

```text
ok   message
--   -------
True Acesso autorizado.
```

## Variável de ambiente de teste

`REGOPTIMIZER_AUTH_URL` tem prioridade sobre `Data/auth.json`. Antes do teste final, confirme que não existe uma URL antiga:

```powershell
$env:REGOPTIMIZER_AUTH_URL
[Environment]::GetEnvironmentVariable("REGOPTIMIZER_AUTH_URL", "User")
```

Se necessário, remova:

```powershell
Remove-Item Env:REGOPTIMIZER_AUTH_URL -ErrorAction SilentlyContinue
[Environment]::SetEnvironmentVariable("REGOPTIMIZER_AUTH_URL", $null, "User")
```
