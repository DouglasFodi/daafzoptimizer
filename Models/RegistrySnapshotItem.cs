using Microsoft.Win32;

namespace RegOptimizer.Models;

public sealed class RegistrySnapshot
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string OperationLabel { get; set; } = "";
    public List<RegistrySnapshotItem> Items { get; set; } = new();
}

public sealed class RegistrySnapshotItem
{
    public string TweakId { get; set; } = "";
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Existed { get; set; }
    public RegistryValueKind Kind { get; set; }
    public string ValueType { get; set; } = "";
    public string? StringValue { get; set; }
    public long? NumberValue { get; set; }
    public string? BinaryBase64 { get; set; }
    public string[]? MultiStringValue { get; set; }
}
