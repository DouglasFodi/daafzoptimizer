using System.Text.Json;
using Microsoft.Win32;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class RegistryOperationService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<RegistryOperation> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "registry_operations.json");
        if (!File.Exists(path)) return new List<RegistryOperation>();
        return JsonSerializer.Deserialize<List<RegistryOperation>>(File.ReadAllText(path), Options)
               ?? new List<RegistryOperation>();
    }

    public static List<RegistryOperationSnapshotItem> Capture(IEnumerable<RegistryOperation> operations) =>
        operations.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                  .Select(g => CaptureOne(g.First()))
                  .ToList();

    public static void Apply(IEnumerable<RegistryOperation> operations)
    {
        // Criações primeiro; exclusões depois. Isso torna a ordem determinística.
        foreach (var op in operations.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                                     .Select(g => g.First())
                                     .OrderBy(x => x.OperationType == "CreateKey" ? 0 : 1))
        {
            ApplyOne(op);
            VerifyOne(op);
        }
    }

    public static void Restore(IEnumerable<RegistryOperation> operations, RegistrySnapshot snapshot)
    {
        var wanted = operations.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = snapshot.Operations
            .Where(x => wanted.Contains(x.OperationId))
            .ToDictionary(x => x.OperationId, StringComparer.OrdinalIgnoreCase);

        // Ordem inversa da aplicação.
        foreach (var op in operations.Reverse())
        {
            if (!map.TryGetValue(op.Id, out var snap)) continue;
            RestoreOne(snap);
        }
    }

    private static RegistryOperationSnapshotItem CaptureOne(RegistryOperation op)
    {
        var (hive, subKey) = SplitPath(op.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var key = baseKey.OpenSubKey(subKey, writable: false);

        return new RegistryOperationSnapshotItem
        {
            OperationId = op.Id,
            OperationType = op.OperationType,
            Path = op.Path,
            Existed = key is not null,
            Tree = key is null ? null : CaptureTree(key)
        };
    }

    private static void ApplyOne(RegistryOperation op)
    {
        var (hive, subKey) = SplitPath(op.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);

        switch (op.OperationType)
        {
            case "CreateKey":
                using (var key = baseKey.CreateSubKey(subKey, writable: true))
                {
                    if (key is null)
                        throw new InvalidOperationException($"Não foi possível criar a chave {op.Path}");
                    key.Flush();
                }
                break;

            case "DeleteKey":
                baseKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
                break;

            default:
                throw new NotSupportedException($"Operação estrutural não suportada: {op.OperationType} ({op.Id})");
        }
    }

    private static void VerifyOne(RegistryOperation op)
    {
        var (hive, subKey) = SplitPath(op.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var key = baseKey.OpenSubKey(subKey, writable: false);

        if (op.OperationType == "CreateKey" && key is null)
            throw new InvalidOperationException($"Validação falhou: a chave não foi criada: {op.Path}");

        if (op.OperationType == "DeleteKey" && key is not null)
            throw new InvalidOperationException($"Validação falhou: a chave ainda existe após exclusão: {op.Path}");
    }

    private static void RestoreOne(RegistryOperationSnapshotItem snap)
    {
        var (hive, subKey) = SplitPath(snap.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);

        if (!snap.Existed)
        {
            // Se a chave foi criada pela operação e não existia antes, remove tudo que a operação criou.
            baseKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
            return;
        }

        // Se existia, recria exatamente a árvore capturada antes da alteração.
        baseKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Não foi possível restaurar a chave {snap.Path}");
        RestoreTree(key, snap.Tree ?? new RegistryKeyTreeSnapshot());
        key.Flush();
    }

    private static RegistryKeyTreeSnapshot CaptureTree(RegistryKey key)
    {
        var tree = new RegistryKeyTreeSnapshot();

        foreach (var name in key.GetValueNames())
        {
            var kind = key.GetValueKind(name);
            var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            var item = new RegistryTreeValueSnapshot { Name = name, Kind = kind };
            switch (value)
            {
                case int i:
                    item.ValueType = "Int32"; item.NumberValue = i; break;
                case long l:
                    item.ValueType = "Int64"; item.NumberValue = l; break;
                case byte[] bytes:
                    item.ValueType = "Binary"; item.BinaryBase64 = Convert.ToBase64String(bytes); break;
                case string[] arr:
                    item.ValueType = "MultiString"; item.MultiStringValue = arr; break;
                default:
                    item.ValueType = "String"; item.StringValue = value?.ToString(); break;
            }
            tree.Values.Add(item);
        }

        foreach (var subName in key.GetSubKeyNames())
        {
            using var sub = key.OpenSubKey(subName, writable: false);
            if (sub is not null)
                tree.SubKeys[subName] = CaptureTree(sub);
        }

        return tree;
    }

    private static void RestoreTree(RegistryKey key, RegistryKeyTreeSnapshot tree)
    {
        foreach (var item in tree.Values)
        {
            object value = item.ValueType switch
            {
                "Int32" => unchecked((int)(item.NumberValue ?? 0)),
                "Int64" => item.NumberValue ?? 0L,
                "Binary" => Convert.FromBase64String(item.BinaryBase64 ?? ""),
                "MultiString" => item.MultiStringValue ?? Array.Empty<string>(),
                _ => item.StringValue ?? ""
            };
            key.SetValue(item.Name, value, item.Kind);
        }

        foreach (var pair in tree.SubKeys)
        {
            using var sub = key.CreateSubKey(pair.Key, writable: true)
                ?? throw new InvalidOperationException($"Não foi possível restaurar a subchave {pair.Key}");
            RestoreTree(sub, pair.Value);
            sub.Flush();
        }
    }

    private static (RegistryHive hive, string subKey) SplitPath(string path)
    {
        var idx = path.IndexOf('\\');
        var root = idx < 0 ? path : path[..idx];
        var sub = idx < 0 ? "" : path[(idx + 1)..];
        var hive = root.ToUpperInvariant() switch
        {
            "HKEY_LOCAL_MACHINE" or "HKLM" => RegistryHive.LocalMachine,
            "HKEY_CURRENT_USER" or "HKCU" => RegistryHive.CurrentUser,
            "HKEY_CLASSES_ROOT" or "HKCR" => RegistryHive.ClassesRoot,
            "HKEY_USERS" or "HKU" => RegistryHive.Users,
            "HKEY_CURRENT_CONFIG" or "HKCC" => RegistryHive.CurrentConfig,
            _ => throw new NotSupportedException($"Hive não suportada: {root}")
        };
        return (hive, sub);
    }
}
