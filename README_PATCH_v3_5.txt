PATCH v3.5 - CORRECAO DE COMPATIBILIDADE + DETECCAO AUTOMATICA

Corrige o conflito causado pelo patch v3.4, que havia substituido modelos da v3.3 por versoes antigas.

Mantido da v3.3:
- RegistryOperation / registry_operations.json
- OperationIds / Operations nas opcoes inteligentes
- RegistryAssignmentDisplay e exibicao @=""
- aplicacao/validacao/rollback das operacoes estruturais
- fallback reg.exe /ve para valor padrao

Adicionado:
- Selecionar tudo em Opcoes Inteligentes (respeita filtros e nao seleciona REVISAR)
- Selecionar tudo em Individual/Avancado (respeita aba e filtros)
- deteccao automatica dos valores ja existentes no PC
- valores detectados ficam marcados automaticamente
- opcoes inteligentes so contam como detectadas quando todos os valores e operacoes estruturais correspondem ao estado desejado
- badges com quantidade detectada
- status JÁ NO PC / NÃO DETECTADA

Arquivos do patch:
MainWindow.xaml
MainWindow.xaml.cs
Models/Tweak.cs
Models/SmartOption.cs
Services/RegistryDetectionService.cs

Antes de compilar:
Stop-Process -Name RegOptimizer -Force -ErrorAction SilentlyContinue
dotnet clean
dotnet build
