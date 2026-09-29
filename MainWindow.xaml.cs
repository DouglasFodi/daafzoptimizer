using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using RegOptimizer.Models;
using RegOptimizer.Services;

namespace RegOptimizer;

public partial class MainWindow : Window
{
    private readonly List<Tweak> _all = new();
    private readonly Dictionary<string, Tweak> _tweakById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<RegistryOperation> _registryOperations = new();
    private readonly Dictionary<string, RegistryOperation> _operationById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SmartOption> _smartOptions = new();
    private readonly List<SmartProfile> _profiles = new();
    private string _individualTab = "Geral";
    private int _detectedIndividualCount;
    private int _detectedSmartCount;

    public MainWindow()
    {
        InitializeComponent();

        _all = CatalogService.Load();
        _tweakById = _all.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        _registryOperations = RegistryOperationService.Load();
        _operationById = _registryOperations.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        _smartOptions = SmartCatalogService.LoadOptions(_tweakById, _operationById);
        _profiles = SmartCatalogService.LoadProfiles();

        var tabs = new[] { "Geral", "Windows 11", "Gaming", "Rede", "Hardware", "Avançado" };
        TabsList.ItemsSource = tabs;
        TabsList.SelectedIndex = 0;

        SmartCategoryFilter.ItemsSource = new[] { "Todas as categorias" }
            .Concat(_smartOptions.Select(x => x.Category).Distinct().OrderBy(x => x))
            .ToArray();
        SmartCategoryFilter.SelectedIndex = 0;
        SmartRiskFilter.SelectedIndex = 0;
        RiskFilter.SelectedIndex = 0;

        var admin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

        // Detecta o estado real do PC antes da primeira renderização.
        // As otimizações que já correspondem exatamente ao catálogo ficam marcadas.
        DetectInstalledOptimizations(selectDetected: true);

        HeaderInfo.Text =
            $"Windows • {(admin ? "Administrador" : "Sem elevação")} • " +
            $"{_detectedIndividualCount} valor(es) / {_detectedSmartCount} opção(ões) já detectado(s) no PC";

        RefreshSmartView();
        RefreshIndividualView();
        UpdateStatus();
    }

    private bool IsSmartMode => MainModeTabs.SelectedIndex == 0;

    private void DetectInstalledOptimizations(bool selectDetected)
    {
        _detectedIndividualCount = 0;

        foreach (var tweak in _all)
        {
            tweak.IsApplied = RegistryDetectionService.IsApplied(tweak);
            if (tweak.IsApplied)
                _detectedIndividualCount++;

            if (selectDetected)
                tweak.IsSelected = tweak.IsApplied;
        }

        _detectedSmartCount = 0;

        foreach (var option in _smartOptions)
        {
            var actionCount = option.Items.Count + option.Operations.Count;
            option.IsApplied = actionCount > 0
                && option.Items.All(x => x.IsApplied)
                && option.Operations.All(op => RegistryDetectionService.IsApplied(op));

            if (option.IsApplied)
                _detectedSmartCount++;

            if (selectDetected)
                option.IsSelected = option.CanApply && option.IsApplied;
        }

        if (CountBadge is not null)
            CountBadge.Text = $"{_detectedIndividualCount}/{_all.Count} valores no PC";

        if (SmartCountBadge is not null)
            SmartCountBadge.Text = $"{_detectedSmartCount}/{_smartOptions.Count} opções no PC";
    }

    private void MainModeTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != MainModeTabs) return;
        UpdateStatus();
    }

    private void Profile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string id) return;
        var profile = _profiles.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (profile is null) return;

        foreach (var option in _smartOptions)
            option.IsSelected = false;

        var selected = profile.OptionIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var option in _smartOptions.Where(x => x.CanApply && selected.Contains(x.Id)))
            option.IsSelected = true;

        RefreshSmartView();
        UpdateStatus();
        StatusText.Text = $"Perfil \"{profile.Name}\" selecionado. Revise as opções e clique Aplicar quando estiver pronto.";
    }

    private void SmartFilterChanged(object sender, EventArgs e)
    {
        if (!IsLoaded) return;
        RefreshSmartView();
        UpdateStatus();
    }

    private IEnumerable<SmartOption> GetFilteredSmartOptions()
    {
        var q = (SmartSearchBox?.Text ?? "").Trim();
        var category = SmartCategoryFilter?.SelectedItem?.ToString() ?? "Todas as categorias";
        var risk = (SmartRiskFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos os riscos";

        IEnumerable<SmartOption> filtered = _smartOptions;

        if (category != "Todas as categorias")
            filtered = filtered.Where(x => x.Category == category);

        if (risk != "Todos os riscos")
            filtered = filtered.Where(x => x.Risk == risk);

        if (!string.IsNullOrWhiteSpace(q))
        {
            filtered = filtered.Where(o =>
                o.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                o.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                o.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                o.Profile.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                o.Items.Any(t =>
                    t.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.Path.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.ApplyRaw.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                o.Operations.Any(op =>
                    op.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    op.Path.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    op.OperationType.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        return filtered;
    }

    private void RefreshSmartView()
    {
        if (SmartCategoryItems is null) return;

        SmartCategoryItems.ItemsSource = GetFilteredSmartOptions()
            .GroupBy(x => x.Category)
            .OrderBy(g => g.Key)
            .Select(g => new SmartCategoryView
            {
                Name = $"{g.Key} ({g.Count()})",
                Items = g.OrderBy(x => x.ReviewRequired).ThenBy(x => x.Name).ToList()
            })
            .ToList();
    }

    private void SelectAllSmart_Click(object sender, RoutedEventArgs e)
    {
        foreach (var option in GetFilteredSmartOptions().Where(x => x.CanApply))
            option.IsSelected = true;

        RefreshSmartView();
        UpdateStatus();
    }

    private void TabsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabsList.SelectedItem is string s)
        {
            _individualTab = s;
            RefreshIndividualView();
            UpdateStatus();
        }
    }

    private void FilterChanged(object sender, EventArgs e)
    {
        if (!IsLoaded) return;
        RefreshIndividualView();
        UpdateStatus();
    }

    private IEnumerable<Tweak> GetFilteredIndividualTweaks()
    {
        var q = (SearchBox?.Text ?? "").Trim();
        var risk = (RiskFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos os riscos";

        IEnumerable<Tweak> filtered = _all.Where(t => t.Tab == _individualTab);

        if (!string.IsNullOrWhiteSpace(q))
        {
            filtered = filtered.Where(t =>
                t.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Path.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.ApplyRaw.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (risk != "Todos os riscos")
            filtered = filtered.Where(t => t.Risk == risk);

        return filtered;
    }

    private void RefreshIndividualView()
    {
        if (SectionsItems is null) return;

        SectionsItems.ItemsSource = GetFilteredIndividualTweaks()
            .GroupBy(t => t.Section)
            .OrderBy(g => g.Key)
            .Select(g => new SectionView { Name = $"{g.Key} ({g.Count()})", Items = g.ToList() })
            .ToList();
    }

    private bool IsSourceConflict(Tweak tweak)
    {
        return _all
            .Where(x => x.Path.Equals(tweak.Path, StringComparison.OrdinalIgnoreCase)
                     && x.Name.Equals(tweak.Name, StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.RegistryType}\u001f{x.ApplyRaw}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Skip(1)
            .Any();
    }

    private IEnumerable<Tweak> SameRegistryTarget(Tweak tweak) =>
        _all.Where(x => x.Path.Equals(tweak.Path, StringComparison.OrdinalIgnoreCase)
                     && x.Name.Equals(tweak.Name, StringComparison.OrdinalIgnoreCase));

    private void SelectAllIndividual_Click(object sender, RoutedEventArgs e)
    {
        var visible = GetFilteredIndividualTweaks().ToList();
        var skippedConflicts = 0;

        foreach (var tweak in visible)
        {
            if (!IsSourceConflict(tweak))
            {
                tweak.IsSelected = true;
                continue;
            }

            // Para alvos conflitantes do .reg, mantém somente a versão que
            // já corresponde ao estado real do PC. Se nenhuma estiver aplicada,
            // não seleciona nenhuma automaticamente.
            if (tweak.IsApplied)
            {
                foreach (var other in SameRegistryTarget(tweak).Where(x => x.Id != tweak.Id))
                    other.IsSelected = false;

                tweak.IsSelected = true;
            }
            else
            {
                tweak.IsSelected = false;
                skippedConflicts++;
            }
        }

        RefreshIndividualView();
        UpdateStatus();

        if (skippedConflicts > 0)
            StatusText.Text += $" • {skippedConflicts} item(ns) conflitante(s) não selecionado(s) automaticamente";
    }

    private void SmartSelectionChanged(object sender, RoutedEventArgs e) => UpdateStatus();

    private void IndividualSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox
            && checkBox.DataContext is Tweak selected
            && selected.IsSelected
            && IsSourceConflict(selected))
        {
            // Nunca deixa duas definições diferentes do mesmo Path + Name
            // selecionadas ao mesmo tempo. A última marcada pelo usuário vence.
            foreach (var other in SameRegistryTarget(selected).Where(x => x.Id != selected.Id))
                other.IsSelected = false;

            RefreshIndividualView();
        }

        UpdateStatus();
    }

    private List<SmartOption> SelectedSmartOptions() =>
        _smartOptions.Where(x => x.IsSelected && x.CanApply).ToList();

    private List<Tweak> SelectedSmartTweaks() =>
        SelectedSmartOptions()
            .SelectMany(x => x.Items)
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

    private List<RegistryOperation> SelectedSmartOperations() =>
        SelectedSmartOptions()
            .SelectMany(x => x.Operations)
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

    private List<Tweak> SelectedIndividualTweaks() =>
        _all.Where(x => x.IsSelected).ToList();

    private void UpdateStatus()
    {
        if (StatusText is null || SelectionText is null) return;

        if (IsSmartMode)
        {
            var options = SelectedSmartOptions();
            var tweaks = SelectedSmartTweaks();
            var operations = SelectedSmartOperations();
            StatusText.Text = $"{options.Count} opção(ões) inteligente(s) selecionada(s)";
            SelectionText.Text =
                $"{tweaks.Count} valor(es) + {operations.Count} operação(ões) estrutural(is) serão aplicados/restaurados • " +
                $"Detectadas no PC: {_detectedSmartCount} opção(ões) / {_detectedIndividualCount} valor(es)";
        }
        else
        {
            var tweaks = SelectedIndividualTweaks();
            StatusText.Text = $"{tweaks.Count} registro(s) individuais selecionados";
            SelectionText.Text =
                $"Detectadas no PC: {_detectedIndividualCount}/{_all.Count} • " +
                "Modo avançado: conflitos do arquivo de origem são bloqueados pelo mecanismo de aplicação.";
        }
    }

    private void ClearCurrentSelection_Click(object sender, RoutedEventArgs e)
    {
        if (IsSmartMode)
        {
            foreach (var option in _smartOptions) option.IsSelected = false;
            RefreshSmartView();
        }
        else
        {
            foreach (var t in _all) t.IsSelected = false;
            RefreshIndividualView();
        }
        UpdateStatus();
    }

    private void ClearIndividualSelection_Click(object sender, RoutedEventArgs e)
    {
        foreach (var t in _all) t.IsSelected = false;
        RefreshIndividualView();
        UpdateStatus();
    }

    private void ApplySelected_Click(object sender, RoutedEventArgs e)
    {
        var tweaks = IsSmartMode ? SelectedSmartTweaks() : SelectedIndividualTweaks();
        var operations = IsSmartMode ? SelectedSmartOperations() : new List<RegistryOperation>();
        if (tweaks.Count == 0 && operations.Count == 0)
        {
            MessageBox.Show("Selecione ao menos uma opção/registro aplicável.");
            return;
        }

        string label;
        if (IsSmartMode)
        {
            var names = SelectedSmartOptions().Select(x => x.Name).ToList();
            label = names.Count <= 3 ? string.Join(" + ", names) : $"{names.Count} opções inteligentes";
        }
        else
        {
            label = $"{tweaks.Count} registros individuais";
        }

        var highRisk = IsSmartMode
            ? SelectedSmartOptions().Where(x => x.Risk == "Alto").Select(x => x.Name).ToList()
            : tweaks.Where(x => x.Risk == "Alto").Select(x => x.DisplayName).Take(12).ToList();

        if (highRisk.Count > 0)
        {
            var answer = MessageBox.Show(
                "A seleção contém ajustes de ALTO RISCO:\n\n" +
                string.Join("\n", highRisk.Select(x => "• " + x)) +
                "\n\nUm snapshot será criado antes da aplicação. Deseja continuar?",
                "Confirmar ajustes de alto risco",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes) return;
        }

        RegistrySnapshot? snapshot = null;
        string? file = null;
        try
        {
            snapshot = RegistryService.Capture(tweaks, label);
            snapshot.Operations = RegistryOperationService.Capture(operations);
            file = SnapshotService.Save(snapshot);

            var createOps = operations.Where(x => x.OperationType == "CreateKey").ToList();
            var deleteOps = operations.Where(x => x.OperationType == "DeleteKey").ToList();

            RegistryOperationService.Apply(createOps);
            RegistryService.Apply(tweaks);
            RegistryOperationService.Apply(deleteOps);

            DetectInstalledOptimizations(selectDetected: true);
            RefreshSmartView();
            RefreshIndividualView();
            UpdateStatus();

            MessageBox.Show(
                $"Aplicação e validação concluídas.\n\nSnapshot criado antes da alteração:\n{file}\n\n" +
                $"Valores confirmados: {tweaks.Count}\nOperações estruturais confirmadas: {operations.Count}\n" +
                $"Detectados no PC: {_detectedIndividualCount} valores / {_detectedSmartCount} opções inteligentes",
                "RegOptimizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusText.Text = $"Aplicados e validados {tweaks.Count} valores + {operations.Count} operações estruturais. {_detectedSmartCount} opções detectadas no PC.";
        }
        catch (Exception ex)
        {
            string rollback = "";
            if (snapshot is not null)
            {
                try
                {
                    RegistryService.Restore(tweaks, snapshot);
                    RegistryOperationService.Restore(operations, snapshot);
                    rollback = "\n\nAs alterações desta seleção foram revertidas usando o snapshot criado antes da aplicação.";
                }
                catch (Exception rollbackEx)
                {
                    rollback = $"\n\nATENÇÃO: a reversão automática também falhou: {rollbackEx.Message}\nSnapshot: {file}";
                }
            }

            MessageBox.Show(ex.Message + rollback, "Falha ao aplicar/validar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RestoreSelected_Click(object sender, RoutedEventArgs e)
    {
        var tweaks = IsSmartMode ? SelectedSmartTweaks() : SelectedIndividualTweaks();
        var operations = IsSmartMode ? SelectedSmartOperations() : new List<RegistryOperation>();
        if (tweaks.Count == 0 && operations.Count == 0)
        {
            MessageBox.Show("Selecione ao menos uma opção/registro para restaurar.");
            return;
        }

        var ids = tweaks.Select(x => x.Id).ToList();
        var operationIds = operations.Select(x => x.Id).ToList();
        var snapshot = SnapshotService.FindLatestContaining(ids, operationIds);
        if (snapshot is null)
        {
            MessageBox.Show(
                "Não foi encontrado um snapshot compatível contendo todos os registros/operações selecionados.\n\n" +
                "A restauração só é feita quando há dados anteriores exatos para todo o conjunto.",
                "Snapshot não encontrado",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            RegistryService.Restore(tweaks, snapshot);
            RegistryOperationService.Restore(operations, snapshot);

            DetectInstalledOptimizations(selectDetected: true);
            RefreshSmartView();
            RefreshIndividualView();
            UpdateStatus();

            MessageBox.Show(
                $"Restauração concluída.\n\nSnapshot: {snapshot.CreatedAt:G}\n" +
                $"Operação original: {snapshot.OperationLabel}\nValores restaurados: {tweaks.Count}\n" +
                $"Operações estruturais restauradas: {operations.Count}",
                "RegOptimizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Falha ao restaurar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenSnapshots_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(SnapshotService.DirectoryPath);
        Process.Start(new ProcessStartInfo("explorer.exe", SnapshotService.DirectoryPath)
        {
            UseShellExecute = true
        });
    }
}
