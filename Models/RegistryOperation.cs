namespace RegOptimizer.Models;

public sealed class RegistryOperation
{
    public string Id { get; set; } = "";
    public int SourceLine { get; set; }
    public string DisplayName { get; set; } = "";
    public string OperationType { get; set; } = ""; // CreateKey | DeleteKey
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public string Risk { get; set; } = "";

    public string OperationDisplay => OperationType switch
    {
        "CreateKey" => $"Criar chave: [{Path}]",
        "DeleteKey" => $"Excluir chave: [-{Path}]",
        _ => $"{OperationType}: {Path}"
    };
}

public sealed class RegistryOperationSnapshotItem
{
    public string OperationId { get; set; } = "";
    public string OperationType { get; set; } = "";
    public string Path { get; set; } = "";
    public bool Existed { get; set; }
    public RegistryKeyTreeSnapshot? Tree { get; set; }
}

public sealed class RegistryKeyTreeSnapshot
{
    public List<RegistryTreeValueSnapshot> Values { get; set; } = new();
    public Dictionary<string, RegistryKeyTreeSnapshot> SubKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class RegistryTreeValueSnapshot
{
    public string Name { get; set; } = "";
    public Microsoft.Win32.RegistryValueKind Kind { get; set; }
    public string ValueType { get; set; } = "";
    public string? StringValue { get; set; }
    public long? NumberValue { get; set; }
    public string? BinaryBase64 { get; set; }
    public string[]? MultiStringValue { get; set; }
}
