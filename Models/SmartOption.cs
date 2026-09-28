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
    public string Notes { get; set; } = "";
    public bool IsSelected { get; set; }

    [JsonIgnore] public List<Tweak> Items { get; set; } = new();
    [JsonIgnore] public int TweakCount => TweakIds.Count;
    [JsonIgnore] public bool CanApply => !ReviewRequired;
    [JsonIgnore] public string StatusText => ReviewRequired ? "REVISAR" : "APLICÁVEL";
    [JsonIgnore] public string CountText => $"{TweakCount} registro(s)";
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
