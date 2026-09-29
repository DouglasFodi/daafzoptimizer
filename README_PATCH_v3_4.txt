RegOptimizer v3.4 - Detecção automática + Selecionar tudo
==========================================================

Arquivos alterados/adicionados:
- MainWindow.xaml
- MainWindow.xaml.cs
- Models/Tweak.cs
- Models/SmartOption.cs
- Services/RegistryDetectionService.cs (NOVO)

IMPORTANTE:
Este patch NÃO substitui Services/RegistryService.cs.
Portanto, a correção anterior do valor padrão @="" permanece intacta.

Novidades:
1) Ao abrir o programa, as 516 entradas são comparadas com o Registro do Windows.
2) Entradas que já têm caminho + nome + tipo + valor corretos ficam marcadas automaticamente.
3) Uma opção inteligente fica marcada como "JÁ NO PC" somente quando todos os registros dela coincidem.
4) O topo mostra:
   - X/516 registros no PC
   - X/68 opções no PC
5) Botão "Selecionar tudo" em Opções Inteligentes.
   - Respeita busca/categoria/risco visíveis.
   - Não seleciona opções bloqueadas REVISAR.
6) Botão "Selecionar tudo" em Individual / Avançado.
   - Seleciona os registros visíveis na subaba/filtros atuais.
7) Após Aplicar ou Restaurar, o programa relê o Registro e atualiza as marcações/contagens.

COMO INSTALAR
1. Feche completamente o RegOptimizer:
   Stop-Process -Name RegOptimizer -Force -ErrorAction SilentlyContinue

2. Copie o conteúdo deste patch para a raiz do projeto, aceitando substituir arquivos.

3. Compile:
   dotnet clean
   dotnet build

4. Rode:
   dotnet run

5. Para subir ao GitHub:
   git add .
   git commit -m "v3.4 adiciona selecionar tudo e deteccao automatica"
   git push
