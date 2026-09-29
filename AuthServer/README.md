# Servidor de autenticação

O código público contém apenas `auth.php`, `create_hash.php` e `secret.example.php`.
O arquivo real `secret.php` **não deve ser versionado**.

## Produção atual

Endpoint:

```text
https://regoptimizer.duckdns.org/auth.php
```

Arquivos na EC2:

```text
/var/www/html/regoptimizer/auth.php
/var/www/html/regoptimizer/secret.php
```

O Nginx encaminha somente `/auth.php` para o PHP-FPM. O segredo não deve ser exposto pela web.

## Atualizar `auth.php` a partir do GitHub

Depois de um `git pull` na EC2:

```bash
sudo cp AuthServer/auth.php /var/www/html/regoptimizer/auth.php
sudo chown root:www-data /var/www/html/regoptimizer/auth.php
sudo chmod 644 /var/www/html/regoptimizer/auth.php
sudo nginx -t && sudo systemctl reload nginx
```

Não copie `secret.example.php` sobre o `secret.php` de produção.

## Gerar novo hash

```bash
php create_hash.php "SUA-SENHA-FORTE"
```

ou diretamente:

```bash
php -r 'echo password_hash("SUA-SENHA-FORTE", PASSWORD_DEFAULT), PHP_EOL;'
```

## Respostas

Sucesso:

```json
{"ok":true,"message":"Acesso autorizado."}
```

Falha:

```json
{"ok":false,"message":"Senha inválida."}
```
