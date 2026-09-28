namespace RegOptimizer.Models;

public sealed class Tweak
{
    public string Id { get; set; } = "";
    public int SourceLine { get; set; }
    public string DisplayName { get; set; } = "";
    public string Tab { get; set; } = "";
    public string Section { get; set; } = "";
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public string RegistryType { get; set; } = "";
    public string ApplyRaw { get; set; } = "";
    public object? ApplyValue { get; set; }
    public string Description { get; set; } = "";
    public string Risk { get; set; } = "";
    public string OptimizerReference { get; set; } = "";
    public string OptimizerReferenceNote { get; set; } = "";
    public string Warning { get; set; } = "";
    public string RestoreStrategy { get; set; } = "";
    public bool IsSelected { get; set; }
}

public sealed class SectionView
{
    public string Name { get; set; } = "";
    public List<Tweak> Items { get; set; } = new();
}
