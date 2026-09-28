using Microsoft.Win32;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class RegistryService
{
    public static RegistrySnapshot Capture(IEnumerable<Tweak> tweaks, string operationLabel = "")
    {
        var list = Deduplicate(tweaks).ToList();
        ValidateNoConflicts(list);
        var snap = new RegistrySnapshot { OperationLabel = operationLabel };
        foreach (var t in list)
            snap.Items.Add(CaptureOne(t));
        return snap;
    }

    public static void Apply(IEnumerable<Tweak> tweaks)
    {
        ValidateNoConflicts(tweaks);
        foreach (var t in Deduplicate(tweaks))
            ApplyOne(t);
    }

    public static void Restore(IEnumerable<Tweak> tweaks, RegistrySnapshot snapshot)
    {
        var wanted = tweaks.Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in snapshot.Items.Where(x => wanted.Contains(x.TweakId)))
            RestoreOne(item);
    }

    private static IEnumerable<Tweak> Deduplicate(IEnumerable<Tweak> tweaks) =>
        tweaks.GroupBy(t => $"{t.Path}\u001f{t.Name}\u001f{t.ApplyRaw}", StringComparer.OrdinalIgnoreCase)
              .Select(g => g.First());

    private static void ValidateNoConflicts(IEnumerable<Tweak> tweaks)
    {
        var conflicts = tweaks
            .GroupBy(t => $"{t.Path}\u001f{t.Name}", StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => x.ApplyRaw).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .ToList();

        if (conflicts.Count > 0)
            throw new InvalidOperationException("Seleção contém chaves conflitantes. Revise os itens marcados como CONFLITO antes de aplicar.");
    }

    private static RegistrySnapshotItem CaptureOne(Tweak t)
    {
        var (hive, subKey) = SplitPath(t.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var key = baseKey.OpenSubKey(subKey, writable: false);

        var item = new RegistrySnapshotItem { TweakId=t.Id, Path=t.Path, Name=t.Name };
        if (key is null) return item;

        string valueName = t.Name == "@" ? "" : t.Name;
        var names = key.GetValueNames();
        if (!names.Contains(valueName, StringComparer.OrdinalIgnoreCase)) return item;

        item.Existed = true;
        item.Kind = key.GetValueKind(valueName);
        var value = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
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
        return item;
    }

    private static void ApplyOne(Tweak t)
    {
        var (hive, subKey) = SplitPath(t.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Não foi possível abrir/criar {t.Path}");

        string name = t.Name == "@" ? "" : t.Name;
        object value;
        RegistryValueKind kind;

        switch (t.RegistryType)
        {
            case "DWord":
                kind = RegistryValueKind.DWord;
                value = unchecked((int)Convert.ToUInt32(t.ApplyRaw["dword:".Length..], 16));
                break;
            case "String":
                kind = RegistryValueKind.String;
                value = Unquote(t.ApplyRaw);
                break;
            case "Binary":
                kind = RegistryValueKind.Binary;
                value = ParseBinary(t.ApplyRaw);
                break;
            default:
                throw new NotSupportedException($"Tipo não suportado automaticamente: {t.RegistryType} ({t.Id})");
        }

        key.SetValue(name, value, kind);
    }

    private static void RestoreOne(RegistrySnapshotItem item)
    {
        var (hive, subKey) = SplitPath(item.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        string name = item.Name == "@" ? "" : item.Name;

        if (!item.Existed)
        {
            using var existing = baseKey.OpenSubKey(subKey, writable: true);
            existing?.DeleteValue(name, throwOnMissingValue: false);
            return;
        }

        using var key = baseKey.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Não foi possível restaurar {item.Path}");

        object value = item.ValueType switch
        {
            "Int32" => unchecked((int)(item.NumberValue ?? 0)),
            "Int64" => item.NumberValue ?? 0L,
            "Binary" => Convert.FromBase64String(item.BinaryBase64 ?? ""),
            "MultiString" => item.MultiStringValue ?? Array.Empty<string>(),
            _ => item.StringValue ?? ""
        };
        key.SetValue(name, value, item.Kind);
    }

    private static (RegistryHive hive, string subKey) SplitPath(string path)
    {
        var idx = path.IndexOf('\\');
        var root = idx < 0 ? path : path[..idx];
        var sub = idx < 0 ? "" : path[(idx + 1)..];
        var hive = root.ToUpperInvariant() switch
        {
            "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
            "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
            "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
            "HKEY_USERS" => RegistryHive.Users,
            "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
            _ => throw new NotSupportedException($"Hive não suportada: {root}")
        };
        return (hive, sub);
    }

    private static string Unquote(string raw)
    {
        if (raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"')
            raw = raw[1..^1];
        return raw.Replace("\\\\", "\\").Replace("\\\"", "\"");
    }

    private static byte[] ParseBinary(string raw)
    {
        var pos = raw.IndexOf(':');
        var payload = pos >= 0 ? raw[(pos + 1)..] : raw;
        return payload.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => Convert.ToByte(x.Trim(), 16)).ToArray();
    }
}
