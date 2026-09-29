# Auditoria de fidelidade do `.reg` — RegOptimizer v3.2

Base comparada: `Data/Nova_otimizacao_original.reg` x `Data/tweaks.json` x mecanismo de aplicação.

## Inventário do arquivo original

- 920 linhas físicas.
- 230 cabeçalhos normais de chave `[HKEY...]`.
- 516 atribuições de valores.
- 453 valores `REG_DWORD`.
- 58 valores `REG_SZ`.
- 5 valores `REG_BINARY`.
- 1 criação de chave sem valor próprio: linha 810 (CLSID do menu clássico).
- 2 exclusões completas de chave `[-HKEY...]`: linhas 605 e 606 (TaskCache do Microsoft Edge Update).
- 0 exclusões individuais de valor (`"Nome"=-`).
- 0 `REG_QWORD`, `REG_MULTI_SZ` ou `REG_EXPAND_SZ` no arquivo atual.

## Resultado da comparação das 516 atribuições

As 516 atribuições estão representadas em `tweaks.json`.

A comparação de cada entrada considerou:

- caminho;
- nome do valor;
- tipo;
- conteúdo bruto do `.reg` (`ApplyRaw`).

Foram observadas apenas 8 diferenças textuais de caminho: no `.reg` original algumas chaves usam a abreviação `HKLM`, enquanto o catálogo usa `HKEY_LOCAL_MACHINE`. São o mesmo hive e não representam divergência funcional.

Os 5 valores binários foram comparados byte a byte e o conteúdo do catálogo corresponde ao `.reg`.

O único valor padrão/sem nome é `REG0452`, linha 812:

```reg
[HKEY_CURRENT_USER\Software\Classes\CLSID\{86CA1AA0-34AA-4E8B-A509-50C905BAE2A2}\InprocServer32]
@=""
```

Na v3.2 o motor converte `@` para o nome vazio da API do Registro, grava `REG_SZ` vazio e relê a chave para validar tipo e conteúdo.

## Operações que o parser antigo ignorava

O parser anterior catalogava somente atribuições de valores. Por isso três comandos estruturais do `.reg` não eram representados:

1. Linha 810 — criar a chave pai do CLSID do menu clássico.
2. Linha 605 — excluir `MicrosoftEdgeUpdateTaskMachineCore` do TaskCache.
3. Linha 606 — excluir `MicrosoftEdgeUpdateTaskMachineUA` do TaskCache.

A v3.2 adiciona `Data/registry_operations.json` com `OP0001` a `OP0003` e um mecanismo específico de captura/aplicação/verificação/restauração dessas operações.

A opção **Menu de contexto clássico** agora executa `OP0001` e `REG0452` em conjunto.

As duas exclusões do TaskCache ficaram em uma opção inteligente própria, **Remover TaskCache do Microsoft Edge Update**, marcada como alto risco e fora dos perfis automáticos.

## Validação pós-aplicação

Depois de cada atribuição, o RegOptimizer agora relê o Registro e exige correspondência exata:

- `REG_DWORD`: tipo e valor de 32 bits, incluindo `ffffffff`.
- `REG_SZ`: tipo e texto exatos, inclusive string vazia.
- `REG_BINARY`: comparação byte a byte.
- `@`: exige valor padrão/sem nome realmente existente.
- `CreateKey`: exige que a chave possa ser reaberta.
- `DeleteKey`: exige que a chave não possa mais ser aberta.

Se uma validação falhar, o programa não mostra sucesso; ele tenta reverter a seleção usando o snapshot criado antes da aplicação.

## Conflitos e duplicidades existentes na fonte

O `.reg` possui 3 alvos com dados/tipos conflitantes:

- `MinAnimate`: DWORD 0 e String `"0"`.
- `KeyboardDelay`: String `"0"` e DWORD 0.
- `DEPOff`: DWORD 1 e String `"1"`.

Esses seis registros permanecem isolados no grupo `SOURCE_CONFLICTS`, que exige revisão e não é aplicado por perfis.

Há também 5 grupos duplicados com o mesmo caminho/nome/valor. O mecanismo deduplica essas ocorrências na aplicação.

## Auditor externo do estado real do PC

Foi adicionado `AUDITAR_REGISTRO.ps1`. Ele não altera o Registro. O script lê `tweaks.json` e `registry_operations.json`, verifica o estado atual do Windows e gera um CSV com estados como:

- `OK`
- `CHAVE_AUSENTE`
- `VALOR_AUSENTE`
- `TIPO_DIFERENTE`
- `VALOR_DIFERENTE`
- `AINDA_EXISTE`
- `ERRO`

Uso:

```powershell
powershell -ExecutionPolicy Bypass -File .\AUDITAR_REGISTRO.ps1
```

`OK` significa apenas que o estado atual do Registro coincide com o valor/operação desejado no catálogo. Se uma opção ainda não foi aplicada, é normal que apareça como ausente ou diferente.

## Observação sobre a compilação do teste anterior

Antes de `dotnet clean`/`dotnet build`, feche o RegOptimizer. Um executável aberto mantém `RegOptimizer.exe`/`.dll` bloqueados e impede que a nova versão substitua a antiga. O `COMPILAR_EXE.bat` da v3.2 encerra automaticamente uma instância anterior antes de compilar.

## Limitação de restauração das chaves excluídas

O snapshot das operações `DeleteKey` preserva a árvore de subchaves, nomes, tipos e valores. Ele não preserva ACLs/permissões explícitas de segurança da chave. Por esse motivo as exclusões do TaskCache ficam fora dos perfis automáticos e exigem seleção manual.
