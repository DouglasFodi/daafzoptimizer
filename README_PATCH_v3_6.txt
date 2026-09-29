RegOptimizer v3.6 - correção do Selecionar tudo em Individual / Avançado

Problema corrigido:
- O botão Selecionar tudo podia marcar simultaneamente duas definições conflitantes do mesmo Path + Name.
- Exemplo em Hardware: REG0184 KeyboardDelay REG_SZ "0" e REG0186 KeyboardDelay REG_DWORD 0.

Novo comportamento:
- Itens sem conflito: selecionados normalmente.
- Itens conflitantes: se uma versão já corresponde ao PC, somente ela permanece marcada.
- Se nenhuma versão conflitante corresponde ao PC, nenhuma é selecionada automaticamente.
- Ao marcar manualmente uma versão conflitante, qualquer outra versão do mesmo Path + Name é desmarcada.

Este patch substitui somente MainWindow.xaml.cs e deve ser aplicado sobre a v3.5.
