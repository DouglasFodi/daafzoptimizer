# RegOptimizer — Intelligent Options

Aplicativo Windows (.NET 8/WPF) que transforma o arquivo `Nova otimização(3).reg`
em opções funcionais agrupadas, mantendo acesso individual aos registros.

## Estado desta versão

- **516 registros** individuais catalogados.
- **68 opções inteligentes**.
- **100% de cobertura**: todos os registros aparecem em alguma opção ou grupo de revisão.
- **49 opções aplicáveis** em lote.
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
