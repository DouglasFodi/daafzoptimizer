# Servidor de autenticação

A senha **não** deve ficar no repositório público.

## 1. Gere o hash no VPS

```bash
php create_hash.php "SUA-SENHA-FORTE"
```

Copie o resultado.

## 2. Configure o segredo

### Opção A — recomendada: variável de ambiente

Defina no PHP/FPM/Apache/Nginx/Painel:

```text
REGOPTIMIZER_PASSWORD_HASH=$2y$...
```

### Opção B — simples

No servidor:

```bash
cp secret.example.php secret.php
nano secret.php
```

Cole o hash em `password_hash`. **Não envie `secret.php` para o GitHub.**

## 3. Publique

Coloque `auth.php` e, se usar a opção B, `secret.php` em uma pasta HTTPS, por exemplo:

```text
https://seusite.com/regoptimizer/auth.php
```

No aplicativo, edite `Data/auth.json` e coloque essa URL em `Endpoint`.

## Resposta da API

Sucesso:

```json
{"ok":true,"message":"Acesso autorizado."}
```

Falha:

```json
{"ok":false,"message":"Senha inválida."}
```

O endpoint inclui um limitador simples de tentativas por IP. Para uso com muitos usuários ou alta exposição pública, substitua por Redis/banco/WAF.
