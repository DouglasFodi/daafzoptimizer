# v3.3 — correção forçada do valor padrão `@=""`

O tweak do menu clássico requer três efeitos:

1. criar `HKCU\Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}`;
2. criar a subchave `InprocServer32`;
3. criar **explicitamente o valor padrão/sem nome** da subchave como `REG_SZ` com conteúdo vazio — sintaxe `.reg`: `@=""`.

A v3.3 mantém a gravação pela API .NET, mas agora possui fallback para `reg.exe add ... /ve /t REG_SZ /d "" /f` caso a primeira validação não encontre o valor padrão. Em seguida, a aplicação só é considerada concluída se `GetValueNames()` contiver o nome vazio, o tipo for `REG_SZ` e o conteúdo for string vazia.

Use `TESTAR_MENU_CLASSICO.ps1` para verificar no PC se o valor padrão existe de forma explícita.
