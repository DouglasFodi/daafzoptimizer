# RegOptimizer — opções inteligentes

## Resultado da transformação
- Registros individuais de origem: **516**
- Opções inteligentes: **68**
- Opções aplicáveis em lote: **49**
- Opções bloqueadas para revisão: **19**
- Cobertura: **516/516 registros (100%)**
- As duplicidades e conflitos do `.reg` também são cobertos como grupos de diagnóstico, sem aplicação automática.

## Categorias
- **Atualizações: 3 opção(ões)**
- **Avançado: 1 opção(ões)**
- **Energia: 4 opção(ões)**
- **GPU: 7 opção(ões)**
- **Gaming: 7 opção(ões)**
- **Geral: 1 opção(ões)**
- **Hardware: 3 opção(ões)**
- **Input: 3 opção(ões)**
- **Interface: 6 opção(ões)**
- **Privacidade: 10 opção(ões)**
- **Rede: 7 opção(ões)**
- **Revisão: 2 opção(ões)**
- **Segurança: 5 opção(ões)**
- **Serviços: 3 opção(ões)**
- **Sistema: 6 opção(ões)**

## Perfis de seleção
### Trabalho
Seleciona opções de interface, produtividade e privacidade de menor impacto; não inclui grupos bloqueados para revisão.
- Opções selecionadas pelo perfil: **19**

### Trabalho + Gamer
Perfil Trabalho mais opções de gaming/input de menor ou moderado impacto. Não seleciona itens de alto risco/bloqueados.
- Opções selecionadas pelo perfil: **32**

### Gamer
Seleciona os grupos de gaming, input, GPU e rede classificados como aplicáveis; itens de alto risco continuam visíveis, mas não entram automaticamente.
- Opções selecionadas pelo perfil: **13**

### Experimental
Seleciona grupos experimentais que não estão bloqueados para revisão. Leia cada detalhe antes de aplicar.
- Opções selecionadas pelo perfil: **16**

## Grupos inteligentes
| Categoria | Opção | Registros | Risco | Estado | Perfil |
|---|---|---:|---|---|---|
| Atualizações | Busca de drivers | 1 | Moderado | Aplicável | Experimental |
| Atualizações | Infraestrutura de Windows Update | 3 | Alto | REVISAR / bloqueado | Experimental |
| Atualizações | Windows Update e Delivery Optimization | 7 | Moderado | Aplicável | Experimental |
| Avançado | Kernel / SEHOP / mitigações | 6 | Alto | REVISAR / bloqueado | Experimental |
| Energia | Desativar Power Throttling | 2 | Moderado | Aplicável | Gamer |
| Energia | Exposição de opções de energia | 3 | Moderado | Aplicável | Experimental |
| Energia | Hibernação e Modern Standby | 6 | Alto | REVISAR / bloqueado | Experimental |
| Energia | Latência de energia avançada | 12 | Alto | REVISAR / bloqueado | Experimental |
| GPU | DWM / Direct3D / renderização | 7 | Moderado | Aplicável | Gamer |
| GPU | DirectX / VRR e GPU por aplicativo | 3 | Moderado | Aplicável | Gamer |
| GPU | GPU power/latency avançado | 26 | Alto | REVISAR / bloqueado | Experimental |
| GPU | GPU preemption avançado | 15 | Alto | REVISAR / bloqueado | Experimental |
| GPU | Hardware Accelerated GPU Scheduling | 1 | Moderado | Aplicável | Gamer |
| GPU | NVIDIA DPC / prioridade de thread | 6 | Alto | Aplicável | Experimental |
| GPU | Outros overrides de GPU/driver | 3 | Alto | REVISAR / bloqueado | Experimental |
| Gaming | Desativar Game Bar e capturas | 18 | Baixo | Aplicável | Gamer |
| Gaming | MMCSS — perfil Games | 4 | Moderado | Aplicável | Gamer |
| Gaming | MMCSS — perfil Low Latency | 9 | Moderado | Aplicável | Gamer |
| Gaming | MMCSS — responsividade global | 6 | Moderado | Aplicável | Gamer |
| Gaming | Modo Jogo — revisar valores | 3 | Alto | REVISAR / bloqueado | Gamer |
| Gaming | Prioridade de CPU por jogo | 3 | Moderado | Aplicável | Gamer |
| Gaming | Serviços Xbox | 7 | Moderado | Aplicável | Gamer |
| Geral | Inicialização mais direta | 3 | Baixo | Aplicável | Trabalho |
| Hardware | Gerenciamento de memória avançado | 18 | Alto | REVISAR / bloqueado | Experimental |
| Hardware | Scheduler, foreground e IRQ | 12 | Alto | Aplicável | Gamer |
| Hardware | Timers e coalescing | 8 | Alto | Aplicável | Experimental |
| Input | Filas e prioridade USB/Input | 3 | Alto | Aplicável | Experimental |
| Input | Mouse sem aceleração / resposta | 20 | Moderado | Aplicável | Gamer |
| Input | Teclado e acessibilidade | 9 | Moderado | Aplicável | Gamer |
| Interface | Aparência, vídeo e áudio | 5 | Baixo | Aplicável | Trabalho |
| Interface | Barra de tarefas e Menu Iniciar | 9 | Baixo | Aplicável | Trabalho |
| Interface | Explorer e efeitos visuais | 30 | Baixo | Aplicável | Trabalho |
| Interface | Interface mais responsiva | 5 | Baixo | Aplicável | Trabalho |
| Interface | Menu de contexto clássico | 1 | Baixo | Aplicável | Trabalho |
| Interface | Shell, notificações e histórico | 10 | Baixo | Aplicável | Trabalho |
| Privacidade | Conteúdo web e idioma | 2 | Baixo | Aplicável | Trabalho |
| Privacidade | Navegadores: telemetria e segundo plano | 21 | Moderado | Aplicável | Trabalho |
| Privacidade | OneDrive | 1 | Moderado | Aplicável | Trabalho |
| Privacidade | Permissões de aplicativos | 9 | Moderado | Aplicável | Trabalho |
| Privacidade | Pesquisa local sem conteúdo web | 9 | Baixo | Aplicável | Trabalho |
| Privacidade | Serviços de telemetria e erros | 4 | Moderado | Aplicável | Trabalho |
| Privacidade | Sincronização de configurações | 6 | Moderado | Aplicável | Trabalho |
| Privacidade | Sugestões, anúncios e conteúdo promocional | 24 | Baixo | Aplicável | Trabalho |
| Privacidade | Telemetria do Windows | 30 | Moderado | Aplicável | Trabalho |
| Privacidade | Windows Error Reporting | 4 | Baixo | Aplicável | Trabalho |
| Rede | AFD / NDIS avançado | 5 | Alto | Aplicável | Experimental |
| Rede | Detecção de conectividade | 1 | Moderado | Aplicável | Experimental |
| Rede | MSMQ TCPNoDelay | 2 | Moderado | Aplicável | Experimental |
| Rede | Prioridade de resolução de rede | 5 | Moderado | Aplicável | Experimental |
| Rede | SMB / compartilhamento — revisar | 7 | Alto | REVISAR / bloqueado | Experimental |
| Rede | TCP — baixa latência | 6 | Moderado | Aplicável | Gamer |
| Rede | TCP/IP avançado | 7 | Alto | Aplicável | Experimental |
| Revisão | Conflitos do arquivo de origem | 6 | Alto | REVISAR / bloqueado | Revisão |
| Revisão | Duplicidades do arquivo de origem | 5 | Baixo | REVISAR / bloqueado | Revisão |
| Segurança | Enhanced Storage / TCG | 1 | Alto | REVISAR / bloqueado | Experimental |
| Segurança | Mitigações e integridade de código | 2 | Alto | REVISAR / bloqueado | Experimental |
| Segurança | SmartScreen | 1 | Moderado | Aplicável | Experimental |
| Segurança | UAC — alto impacto | 2 | Alto | REVISAR / bloqueado | Experimental |
| Segurança | Windows Defender — alto impacto | 12 | Alto | REVISAR / bloqueado | Experimental |
| Serviços | Outros serviços do Windows | 18 | Alto | REVISAR / bloqueado | Experimental |
| Serviços | Serviços de atualização de navegadores | 7 | Moderado | Aplicável | Experimental |
| Serviços | Serviços opcionais críticos | 11 | Alto | REVISAR / bloqueado | Experimental |
| Sistema | Pesquisa e indexação | 4 | Alto | REVISAR / bloqueado | Experimental |
| Sistema | Prefetch / SysMain | 8 | Moderado | Aplicável | Experimental |
| Sistema | Separação de processos svchost | 2 | Moderado | Aplicável | Experimental |
| Sistema | Sistema e manutenção | 4 | Moderado | Aplicável | Trabalho |
| Sistema | Storage Sense | 1 | Baixo | Aplicável | Trabalho |
| Sistema | Timeouts de aplicativos e encerramento | 5 | Moderado | Aplicável | Experimental |

## Regras de segurança implementadas
- O modo inteligente não aplica grupos `ReviewRequired`.
- Cada aplicação gera snapshot antes de alterar o Registro.
- A restauração procura o snapshot mais recente que contenha **todos** os IDs selecionados.
- O modo individual continua disponível para inspeção dos 516 registros.
- O mecanismo de Registro recusa seleções com o mesmo `Path + Name` e valores conflitantes.
- Os perfis apenas marcam opções; não aplicam nada automaticamente.

## Grupos mantidos em revisão
- **Infraestrutura de Windows Update** (Atualizações, 3 registros): Agrupa BITS, Delivery Optimization e Update Orchestrator conforme seus valores.
- **Kernel / SEHOP / mitigações** (Avançado, 6 registros): Agrupa overrides de segurança do kernel em múltiplos ControlSets. Bloqueado.
- **Hibernação e Modern Standby** (Energia, 6 registros): Agrupa Hibernate, CsEnabled, PlatformAoAcOverride e opções relacionadas.
- **Latência de energia avançada** (Energia, 12 registros): Agrupa tolerâncias/latências e diagnósticos de energia.
- **GPU power/latency avançado** (GPU, 26 registros): Grande conjunto de latências de GraphicsDrivers/DXGKrnl; bloqueado para aplicação acidental.
- **GPU preemption avançado** (GPU, 15 registros): Agrupa overrides de preempção do Graphics Scheduler. Mantido em revisão.
- **Outros overrides de GPU/driver** (GPU, 3 registros): Overrides NVIDIA/GraphicsDrivers restantes, dependentes de driver/hardware.
- **Modo Jogo — revisar valores** (Gaming, 3 registros): Seu arquivo contém AllowAutoGameMode=0 e AutoGameModeEnabled=1; o grupo é bloqueado até a inconsistência ser resolvida.
- **Gerenciamento de memória avançado** (Hardware, 18 registros): Agrupa paging executive, pools, SystemPages, IoPageLockLimit e outros overrides.
- **SMB / compartilhamento — revisar** (Rede, 7 registros): Inclui EnableOplocks=0 e outros parâmetros LanmanServer; pode prejudicar compartilhamento e trabalho em rede.
- **Conflitos do arquivo de origem** (Revisão, 6 registros): Mesmo caminho/nome aparece com tipos ou dados diferentes no .reg. Esses itens são exibidos para correção manual e não são aplicados em lote.
- **Duplicidades do arquivo de origem** (Revisão, 5 registros): Ocorrências repetidas do mesmo caminho/nome/valor. O mecanismo inteligente usa apenas a primeira ocorrência canônica.
- **Enhanced Storage / TCG** (Segurança, 1 registros): Configuração de ativação de segurança de armazenamento; exige revisão de hardware.
- **Mitigações e integridade de código** (Segurança, 2 registros): Agrupa HVCI/Device Guard e políticas de execução. Exige revisão específica da máquina.
- **UAC — alto impacto** (Segurança, 2 registros): O arquivo desativa UAC/consentimento administrativo. Grupo bloqueado para evitar aplicação acidental.
- **Windows Defender — alto impacto** (Segurança, 12 registros): Agrupa políticas que desativam ou reduzem componentes do Defender. Mantido bloqueado por segurança.
- **Outros serviços do Windows** (Serviços, 18 registros): Demais valores Start do seu arquivo que exigem avaliação por máquina.
- **Serviços opcionais críticos** (Serviços, 11 registros): RDP, Netlogon, BitLocker, biometria, Work Folders e outros serviços que podem ser necessários em alguns PCs.
- **Pesquisa e indexação** (Sistema, 4 registros): Agrupa DisableIndexing, estado de indexação e WSearch. Os valores originais variam por máquina.