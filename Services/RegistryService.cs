using System.Diagnostics;
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
        var list = Deduplicate(tweaks).ToList();
        ValidateNoConflicts(list);

        foreach (var t in list)
        {
            ApplyOne(t);

            // @="" é o valor padrão/sem nome. Em alguns cenários queremos
            // garantir a mesma semântica do reg.exe /ve, então fazemos fallback
            // explícito caso a primeira gravação não seja observada na validação.
            if (!TryVerifyOne(t, out _) && t.Name == "@" && t.RegistryType == "String")
                ApplyDefaultStringWithRegExe(t);

            VerifyOne(t);
        }
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

        var item = new RegistrySnapshotItem { TweakId = t.Id, Path = t.Path, Name = t.Name };
        if (key is null) return item;

        string valueName = ValueName(t);
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

        var (kind, value) = ExpectedValue(t);

        // Em .reg, @ representa o valor padrão/sem nome. A API aceita null ou string vazia.
        if (t.Name == "@")
            key.SetValue(null!, value, kind);
        else
            key.SetValue(t.Name, value, kind);

        key.Flush();
    }

    private static void ApplyDefaultStringWithRegExe(Tweak t)
    {
        var expected = Unquote(t.ApplyRaw);
        var psi = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "reg.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        psi.ArgumentList.Add("add");
        psi.ArgumentList.Add(t.Path);
        psi.ArgumentList.Add("/ve");
        psi.ArgumentList.Add("/t");
        psi.ArgumentList.Add("REG_SZ");
        psi.ArgumentList.Add("/d");
        psi.ArgumentList.Add(expected);
        psi.ArgumentList.Add("/f");

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Não foi possível iniciar reg.exe para criar o valor padrão.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            throw new InvalidOperationException(
                $"Falha ao criar @=\"\" em {t.Path} via reg.exe. Código {process.ExitCode}. {detail.Trim()}");
        }
    }

    private static void VerifyOne(Tweak t)
    {
        if (!TryVerifyOne(t, out var error))
            throw new InvalidOperationException(error);
    }

    private static bool TryVerifyOne(Tweak t, out string error)
    {
        error = "";
        var (hive, subKey) = SplitPath(t.Path);
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key is null)
        {
            error = $"Validação falhou: a chave não existe após aplicação: {t.Path}";
            return false;
        }

        var valueName = ValueName(t);
        var names = key.GetValueNames();
        if (!names.Contains(valueName, StringComparer.OrdinalIgnoreCase))
        {
            var shown = t.Name == "@" ? "(Padrão) / @" : t.Name;
            error = $"Validação falhou: o valor {shown} não foi criado em {t.Path}.";
            return false;
        }

        var (expectedKind, expectedValue) = ExpectedValue(t);
        var actualKind = key.GetValueKind(valueName);
        if (actualKind != expectedKind)
        {
            error = $"Validação falhou em {t.Id}: tipo esperado {expectedKind}, encontrado {actualKind}.\n{t.Path}";
            return false;
        }

        var actualValue = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        bool equal = expectedKind switch
        {
            RegistryValueKind.DWord => actualValue is int ai && expectedValue is int ei && ai == ei,
            RegistryValueKind.String => actualValue is string astr && expectedValue is string estr && astr == estr,
            RegistryValueKind.Binary => actualValue is byte[] ab && expectedValue is byte[] eb && ab.SequenceEqual(eb),
            _ => Equals(actualValue, expectedValue)
        };

        if (!equal)
        {
            var shown = t.Name == "@" ? "(Padrão) / @" : t.Name;
            error =
                $"Validação falhou em {t.Id}: {shown} foi gravado com conteúdo diferente do .reg original.\n" +
                $"Caminho: {t.Path}\nEsperado: {t.RegistryAssignmentDisplay}";
            return false;
        }

        return true;
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
            existing?.Flush();
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
        key.Flush();
    }

    private static string ValueName(Tweak t) => t.Name == "@" ? "" : t.Name;

    private static (RegistryValueKind kind, object value) ExpectedValue(Tweak t)
    {
        return t.RegistryType switch
        {
            "DWord" => (RegistryValueKind.DWord,
                unchecked((int)Convert.ToUInt32(t.ApplyRaw["dword:".Length..], 16))),
            "String" => (RegistryValueKind.String, Unquote(t.ApplyRaw)),
            "Binary" => (RegistryValueKind.Binary, ParseBinary(t.ApplyRaw)),
            _ => throw new NotSupportedException($"Tipo não suportado automaticamente: {t.RegistryType} ({t.Id})")
        };
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
