# EC2 — autenticação do RegOptimizer

Estado de produção esperado:

- domínio: `regoptimizer.duckdns.org`
- Nginx: portas 80/443
- PHP-FPM: `php8.3-fpm.sock`
- endpoint: `https://regoptimizer.duckdns.org/auth.php`
- `auth.php`: `/var/www/html/regoptimizer/auth.php`
- `secret.php`: `/var/www/html/regoptimizer/secret.php`

## Testes

GET deve ser recusado pelo PHP:

```bash
curl -i https://regoptimizer.duckdns.org/auth.php
```

Esperado: `405 Method Not Allowed`.

POST com senha inválida deve retornar `401`:

```bash
curl -i -X POST \
  "https://regoptimizer.duckdns.org/auth.php" \
  -H "Content-Type: application/json" \
  -d '{"password":"TESTE","application":"RegOptimizer","version":"3.8"}'
```

POST válido deve retornar `200 OK` e `{"ok":true,...}`.
