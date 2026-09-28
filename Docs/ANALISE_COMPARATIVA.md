# Varredura e comparação — hellzerg/optimizer × Nova otimização(3).reg

## Escopo
A estrutura foi inspirada nos padrões de organização observados no repositório público `hellzerg/optimizer`, sem copiar o código-fonte ou a identidade do projeto. O aplicativo deste pacote usa os valores do `.reg` fornecido e uma arquitetura própria baseada em catálogo JSON e snapshot.

## Estrutura observada no Optimizer
- UI WinForms com formulário principal, controles reutilizáveis `ToggleCard` e abas customizadas.
- 22 `TabPage` declaradas no formulário principal e 80 controles `ToggleCard` no código analisado.
- 150 métodos de otimização/reversão encontrados em `OptimizeHelper.cs`.
- Abas/áreas relevantes: General, Windows 10/11, Advanced, UWP Apps, Startup, Apps, Cleaner, Network, Hosts, Registry, Integrator e Options.
- A área Windows separa ajustes em grupos como System, Privacy, Gaming, Touch, Taskbar e Extras.
- O template Windows 11 separa `AdvancedTweaks` de `Tweaks`, o que inspirou a divisão deste projeto.

## Seu arquivo
- Valores ativos analisados: **516**
- Caminhos de Registro distintos: **216**
- Tipos: **453 DWORD**, **58 String**, **5 Binary**
- Duplicidades idênticas: **5 grupos**
- Conflitos de mesmo caminho/nome com dados/tipos diferentes: **3 grupos**
- Entradas com correspondência funcional/direta identificada no Optimizer: **38**

## Distribuição criada
- **Hardware: 161**
  - CPU / Memória / Scheduler: 43
  - Energia / Power Management: 23
  - GPU / Gráficos / Drivers: 61
  - Mouse / Teclado / USB / Input: 34
- **Windows 11: 202**
  - Armazenamento / Storage Sense: 1
  - Explorer / Desktop / Barra de tarefas: 51
  - Explorer / Shell / Interface: 11
  - Gaming / Xbox / Game Bar: 20
  - Menu de contexto clássico: 1
  - Navegadores / Telemetria: 25
  - Pesquisa / Indexação / Cortana: 13
  - Privacidade / Permissões de aplicativos: 9
  - Privacidade / Sincronização: 6
  - Privacidade / Sugestões / Conteúdo: 16
  - Privacidade / Telemetria: 21
  - Privacidade / Web / Idioma: 2
  - Segurança / Defender / UAC / VBS: 17
  - Tema / Aparência: 3
  - Vídeo / Multimídia: 1
  - Windows Update / Delivery Optimization: 4
  - Áudio / Multimídia: 1
- **Gaming: 22**
  - MMCSS / Prioridade multimídia: 13
  - MMCSS / Responsividade do sistema: 6
  - Perfis por jogo / prioridade de processo: 3
- **Geral: 62**
  - Cache / Prefetch / SysMain: 4
  - Inicialização / Startup: 2
  - Serviços do Windows: 52
  - Sistema / Manutenção: 4
- **Avançado: 34**
  - Kernel / Mitigações / Compatibilidade: 7
  - Políticas do Sistema: 27
- **Rede: 35**
  - SMB / Compartilhamento: 7
  - TCP/IP / AFD / Latência: 28

## Diferenças importantes encontradas
| Chave | Seu valor | Referência no Optimizer | Observação |
|---|---:|---|---|
| `HungAppTimeout` | `"4000"` | Optimize Performance | Valor diferente: Optimizer usa 1000; seu arquivo usa 4000. |
| `LowLevelHooksTimeout` | `dword:00001000` | Optimize Performance | Valor diferente: Optimizer usa 1000; seu arquivo usa 4096. |
| `SystemResponsiveness` | `dword:00000014` | Optimize Performance | Divergente: Optimizer usa 1 no modo ativo e 14 ao desfazer; seu arquivo usa 20. |
| `AllowAutoGameMode` | `dword:00000000` | Gaming Mode | Divergente: Optimizer ativa com 1; seu arquivo usa 0. |
| `AutoGameModeEnabled` | `dword:00000001` | Gaming Mode | Mesmo valor de ativação (1). |
| `GameDVR_FSEBehaviorMode` | `dword:00000000` | Gaming Mode | Divergente: Optimizer ativa com 2; seu arquivo usa 0. |
| `HwSchMode` | `dword:00000002` | Gaming Mode | Mesmo valor de ativação no Optimizer (2). |
| `Serviço WSearch — Inicialização` | `dword:00000001` | Disable/Enable Search | Divergente: Optimizer usa Start=4 para desativar e Start=2 para ativar; seu arquivo usa Start=1. |

### Inconsistência interna relevante
No seu arquivo, `AllowAutoGameMode=0` e `AutoGameModeEnabled=1` aparecem simultaneamente. No Optimizer, o par é mantido coerente: ambos 1 ao ativar Gaming Mode e ambos 0 ao desativar.

### Duplicidades/conflitos preservados e sinalizados
- **CONFLITO** `HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics\MinAnimate`: dword:00000000, "0"
- **CONFLITO** `HKEY_CURRENT_USER\Control Panel\Keyboard\KeyboardDelay`: "0", dword:00000000
- **CONFLITO** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Internet Explorer\Main\DEPOff`: dword:00000001, "1"
- **DUPLICADO** `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSMQ\Parameters\TCPNoDelay` = `dword:00000001` (2 ocorrências)
- **DUPLICADO** `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSMQ\TCPNoDelay` = `dword:00000001` (2 ocorrências)
- **DUPLICADO** `HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\XboxNetApiSvc\Start` = `dword:00000004` (2 ocorrências)
- **DUPLICADO** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge\PersonalizationReportingEnabled` = `dword:00000000` (2 ocorrências)
- **DUPLICADO** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge\EdgeCollectionsEnabled` = `dword:00000000` (2 ocorrências)

## Restauração
O projeto não presume que `0` seja o valor original. Antes de aplicar qualquer seleção, ele captura existência, tipo e dados exatos do Registro. Ao restaurar, recria o valor anterior ou remove o valor se ele não existia. Isso é mais robusto para serviços, drivers, GPU, políticas e preferências dependentes da máquina.

## Observação de segurança
A categorização indica onde cada chave pertence funcionalmente; ela não certifica que todos os tweaks sejam benéficos. Itens de serviço, kernel, mitigação, Defender/UAC, drivers, rede e energia foram classificados com risco mais alto/moderado e devem ser testados individualmente.