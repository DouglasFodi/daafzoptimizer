# RegOptimizer — Intelligent Options

Aplicativo Windows (.NET 8/WPF) que transforma o arquivo `Nova otimização(3).reg`
em opções funcionais agrupadas, mantendo acesso individual aos registros.

## Estado desta versão

- **516 registros** individuais catalogados.
- **69 opções inteligentes**.
- **100% de cobertura**: todos os registros aparecem em alguma opção ou grupo de revisão.
- **50 opções aplicáveis** em lote.
- **19 opções bloqueadas para revisão**.
- Perfis: Trabalho, Trabalho + Gamer, Gamer e Experimental.
- Snapshot automático antes de cada aplicação.
- Restauração por snapshot compatível, não por um valor genérico.
- Busca, filtro por categoria e risco.
- Área `Individual / Avançado` preservando as 516 entradas.

## Como funciona

`Data/tweaks.json`
: catálogo de cada registro individual.

`Data/smart_options.json`
: agrupamento funcional dos IDs dos registros.

`Data/profiles.json`
: conjuntos de opções que o usuário pode selecionar rapidamente.

`RegistryService`
: captura, aplica e restaura o Registro.

`SnapshotService`
: salva snapshots e encontra o snapshot compatível mais recente para uma restauração.

## Compilar

Requisitos: Windows 10/11 e .NET 8 SDK.

```powershell
dotnet restore
dotnet build -c Release
dotnet run
```

O manifesto solicita execução como administrador.

## Importante

Perfis apenas **selecionam** opções. Nada é aplicado automaticamente.

Os grupos marcados `REVISAR` ficam bloqueados no modo inteligente porque contêm
configurações de alto impacto, dependentes de hardware/serviço ou inconsistências
detectadas no arquivo de origem.

Leia `Docs/OPCOES_INTELIGENTES.md`.

## Autenticação por senha (projeto público)

Esta versão abre uma tela de autenticação antes da interface principal. A senha não é codificada no executável nem no repositório: o cliente envia a senha via HTTPS para o endpoint configurado em `Data/auth.json`.

Veja `AUTH_SETUP.md` e `AuthServer/README.md` antes de publicar.

Para compilar no Windows, também é possível dar dois cliques em `COMPILAR_EXE.bat`.

## v3.2 — fidelidade do `.reg` e validação pós-aplicação

Esta revisão audita o arquivo `Data/Nova_otimizacao_original.reg` completo.

- 516 atribuições de valores preservadas: 453 DWORD, 58 String e 5 Binary.
- `@=""` tratado explicitamente como valor padrão/sem nome `REG_SZ` vazio.
- 1 criação de chave sem valor e 2 comandos `[-HKEY...]` passaram a ser representados em `Data/registry_operations.json`.
- Cada valor aplicado é relido e comparado com tipo/conteúdo esperado antes de o programa informar sucesso.
- Operações estruturais também são verificadas.
- Falhas tentam restauração automática usando o snapshot prévio.
- `AUDITAR_REGISTRO.ps1` permite comparar o estado real do PC com o catálogo sem alterar o Registro.
- `COMPILAR_EXE.bat` fecha uma instância anterior do RegOptimizer para evitar o erro de arquivo bloqueado durante o build.

Consulte `Docs/AUDITORIA_FIDELIDADE_REG.md` para o relatório técnico.

## v3.3 — valor padrão do menu clássico

O valor `@=""` de `InprocServer32` agora possui gravação reforçada: API .NET + fallback `reg.exe /ve` + validação posterior de existência, tipo e conteúdo.

## v3.4–v3.8 — detecção automática, seleção segura e autenticação EC2

- Ao abrir, o aplicativo lê o Registro e marca automaticamente os valores já aplicados no PC.
- As opções inteligentes só aparecem como detectadas quando todos os valores/operações do grupo estão no estado esperado.
- Os contadores do topo mostram quantos valores e opções inteligentes já existem no computador.
- Botão **Selecionar tudo** nas abas `Opções Inteligentes` e `Individual / Avançado`, respeitando pesquisa/filtros.
- Entradas `REVISAR` continuam bloqueadas no modo inteligente.
- Conflitos de origem com o mesmo `Path + Name` e valores/tipos diferentes não são selecionados juntos; se uma variante já estiver aplicada, somente ela é mantida.
- A seleção manual de uma variante conflitante desmarca a outra automaticamente.
- O endpoint padrão de autenticação está configurado para `https://regoptimizer.duckdns.org/auth.php`. O servidor Nginx/PHP-FPM da EC2 faz a validação e o segredo permanece somente no servidor, nunca no GitHub.

