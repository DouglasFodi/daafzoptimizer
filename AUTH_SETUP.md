# Configuração do acesso por senha

Esta versão do RegOptimizer exige autenticação antes de abrir a janela principal.

## Por que a senha não está no código

Como o projeto pode ficar público no GitHub, qualquer senha fixa ou hash de verificação local poderia ser copiado, testado offline ou removido do código. Por isso, a versão oficial valida a senha em um servidor externo.

> Importante: como o **código-fonte é público**, uma pessoa ainda pode criar um fork, remover a tela de login e compilar outra versão. A autenticação protege o seu executável oficial; ela não torna código público impossível de modificar.

## Configuração rápida

1. Hospede `AuthServer/auth.php` em um domínio/VPS com HTTPS.
2. Gere um hash com:

```bash
php AuthServer/create_hash.php "SUA-SENHA-FORTE"
```

3. Salve o hash no servidor como `REGOPTIMIZER_PASSWORD_HASH` ou em um `AuthServer/secret.php` não versionado.
4. Edite `Data/auth.json`:

```json
{
  "Endpoint": "https://seusite.com/regoptimizer/auth.php",
  "ApplicationId": "RegOptimizer",
  "TimeoutSeconds": 12,
  "RequireHttps": true
}
```

5. Compile/publice normalmente.

## Trocar a senha

Não precisa recompilar o programa. Gere outro hash e substitua somente o segredo no servidor.

## Variável de ambiente no cliente

Para testes, `REGOPTIMIZER_AUTH_URL` substitui o endpoint de `Data/auth.json`:

```powershell
$env:REGOPTIMIZER_AUTH_URL="https://seusite.com/regoptimizer/auth.php"
dotnet run
```

A senha nunca é salva no Windows pelo aplicativo.
