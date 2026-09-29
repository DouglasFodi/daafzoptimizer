# Correção REG0452 — valor padrão @=""

O tweak do menu de contexto clássico precisa criar:

```reg
[HKEY_CURRENT_USER\Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}]

[HKEY_CURRENT_USER\Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}\InprocServer32]
@=""
```

Na versão corrigida, `@` é tratado explicitamente como o valor padrão/sem nome da chave.
O `RegistryService` grava o valor como `REG_SZ` vazio e reabre a chave para verificar que o valor foi materializado.
Se a verificação falhar, o programa apresenta erro e não informa aplicação concluída.

Use `TESTAR_MENU_CLASSICO.ps1` para verificar o resultado após aplicar a opção.
