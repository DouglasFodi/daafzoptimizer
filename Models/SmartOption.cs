using System.Text.Json.Serialization;

namespace RegOptimizer.Models;

public sealed class SmartOption
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Risk { get; set; } = "";
    public string Profile { get; set; } = "";
    public bool ReviewRequired { get; set; }
    public List<string> TweakIds { get; set; } = new();
    public List<string> OperationIds { get; set; } = new();
    public string Notes { get; set; } = "";
    public bool IsSelected { get; set; }

    [JsonIgnore] public List<Tweak> Items { get; set; } = new();
    [JsonIgnore] public List<RegistryOperation> Operations { get; set; } = new();
    [JsonIgnore] public int TweakCount => TweakIds.Count;
    [JsonIgnore] public int OperationCount => OperationIds.Count;
    [JsonIgnore] public int TotalActionCount => TweakCount + OperationCount;
    [JsonIgnore] public bool HasOperations => OperationCount > 0;
    [JsonIgnore] public bool CanApply => !ReviewRequired;
    [JsonIgnore] public bool IsApplied { get; set; }
    [JsonIgnore] public string StatusText => ReviewRequired ? "REVISAR" : IsApplied ? "JÁ NO PC" : "APLICÁVEL";
    [JsonIgnore] public string StatusBackground => ReviewRequired ? "#3F1D1D" : IsApplied ? "#163A2A" : "#27272A";
    [JsonIgnore] public string StatusForeground => ReviewRequired ? "#FCA5A5" : IsApplied ? "#86EFAC" : "#D4D4D8";
    [JsonIgnore] public string CountText => OperationCount == 0
        ? $"{TweakCount} registro(s)"
        : $"{TweakCount} registro(s) + {OperationCount} operação(ões)";
}

public sealed class SmartProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> OptionIds { get; set; } = new();
}

public sealed class SmartCategoryView
{
    public string Name { get; set; } = "";
    public List<SmartOption> Items { get; set; } = new();
}
