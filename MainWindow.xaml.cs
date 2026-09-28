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
    private readonly List<SmartOption> _smartOptions = new();
    private readonly List<SmartProfile> _profiles = new();
    private string _individualTab = "Geral";

    public MainWindow()
    {
        InitializeComponent();

        _all = CatalogService.Load();
        _tweakById = _all.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        _smartOptions = SmartCatalogService.LoadOptions(_tweakById);
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

        HeaderInfo.Text = $"Windows • {(admin ? "Administrador" : "Sem elevação")} • snapshot antes de cada aplicação";
        CountBadge.Text = $"{_all.Count} registros";
        SmartCountBadge.Text = $"{_smartOptions.Count} opções inteligentes";

        RefreshSmartView();
        RefreshIndividualView();
        UpdateStatus();
    }

    private bool IsSmartMode => MainModeTabs.SelectedIndex == 0;

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

    private void RefreshSmartView()
    {
        if (SmartCategoryItems is null) return;

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
                    t.ApplyRaw.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        SmartCategoryItems.ItemsSource = filtered
            .GroupBy(x => x.Category)
            .OrderBy(g => g.Key)
            .Select(g => new SmartCategoryView
            {
                Name = $"{g.Key} ({g.Count()})",
                Items = g.OrderBy(x => x.ReviewRequired).ThenBy(x => x.Name).ToList()
            })
            .ToList();
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

    private void RefreshIndividualView()
    {
        if (SectionsItems is null) return;

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

        SectionsItems.ItemsSource = filtered
            .GroupBy(t => t.Section)
            .OrderBy(g => g.Key)
            .Select(g => new SectionView { Name = $"{g.Key} ({g.Count()})", Items = g.ToList() })
            .ToList();
    }

    private void SmartSelectionChanged(object sender, RoutedEventArgs e) => UpdateStatus();
    private void IndividualSelectionChanged(object sender, RoutedEventArgs e) => UpdateStatus();

    private List<SmartOption> SelectedSmartOptions() =>
        _smartOptions.Where(x => x.IsSelected && x.CanApply).ToList();

    private List<Tweak> SelectedSmartTweaks() =>
        SelectedSmartOptions()
            .SelectMany(x => x.Items)
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
            StatusText.Text = $"{options.Count} opção(ões) inteligente(s) selecionada(s)";
            SelectionText.Text = $"{tweaks.Count} registro(s) serão aplicados/restaurados após deduplicação";
        }
        else
        {
            var tweaks = SelectedIndividualTweaks();
            StatusText.Text = $"{tweaks.Count} registro(s) individuais selecionados";
            SelectionText.Text = "Modo avançado: conflitos do arquivo de origem são bloqueados pelo mecanismo de aplicação.";
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
        if (tweaks.Count == 0)
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

        try
        {
            var snapshot = RegistryService.Capture(tweaks, label);
            var file = SnapshotService.Save(snapshot);
            RegistryService.Apply(tweaks);

            MessageBox.Show(
                $"Aplicação concluída.\n\nSnapshot criado antes da alteração:\n{file}\n\n" +
                $"Registros processados: {tweaks.Count}",
                "RegOptimizer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            StatusText.Text = $"Aplicados {tweaks.Count} registros com snapshot prévio.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Falha ao aplicar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RestoreSelected_Click(object sender, RoutedEventArgs e)
    {
        var tweaks = IsSmartMode ? SelectedSmartTweaks() : SelectedIndividualTweaks();
        if (tweaks.Count == 0)
        {
            MessageBox.Show("Selecione ao menos uma opção/registro para restaurar.");
            return;
        }

        var ids = tweaks.Select(x => x.Id).ToList();
        var snapshot = SnapshotService.FindLatestContaining(ids);
        if (snapshot is null)
        {
            MessageBox.Show(
                "Não foi encontrado um snapshot compatível contendo todos os registros selecionados.\n\n" +
                "A restauração só é feita quando há dados anteriores exatos para todo o conjunto.",
                "Snapshot não encontrado",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            RegistryService.Restore(tweaks, snapshot);
            MessageBox.Show(
                $"Restauração concluída.\n\nSnapshot: {snapshot.CreatedAt:G}\n" +
                $"Operação original: {snapshot.OperationLabel}\nRegistros restaurados: {tweaks.Count}",
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
