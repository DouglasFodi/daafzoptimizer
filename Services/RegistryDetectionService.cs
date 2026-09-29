using Microsoft.Win32;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

/// <summary>
/// Leitura somente do Registro para detectar se cada tweak já está exatamente aplicado.
/// Não cria, altera ou remove chaves/valores.
/// </summary>
public static class RegistryDetectionService
{
    public static bool IsApplied(Tweak tweak)
    {
        try
        {
            var (hive, subKey) = SplitPath(tweak.Path);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey, writable: false);
            if (key is null) return false;

            var valueName = tweak.Name == "@" ? "" : tweak.Name;

            // É importante conferir a existência explicitamente. Em especial para @="",
            // GetValue() sozinho não diferencia valor padrão inexistente de string vazia.
            if (!key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
                return false;

            var actualKind = key.GetValueKind(valueName);
            var actualValue = key.GetValue(
                valueName,
                null,
                RegistryValueOptions.DoNotExpandEnvironmentNames);

            return tweak.RegistryType switch
            {
                "DWord" =>
                    actualKind == RegistryValueKind.DWord &&
                    actualValue is int actualDword &&
                    actualDword == unchecked((int)Convert.ToUInt32(tweak.ApplyRaw["dword:".Length..], 16)),

                "String" =>
                    actualKind == RegistryValueKind.String &&
                    actualValue is string actualString &&
                    string.Equals(actualString, Unquote(tweak.ApplyRaw), StringComparison.Ordinal),

                "Binary" =>
                    actualKind == RegistryValueKind.Binary &&
                    actualValue is byte[] actualBytes &&
                    actualBytes.SequenceEqual(ParseBinary(tweak.ApplyRaw)),

                _ => false
            };
        }
        catch
        {
            // Uma chave inacessível ou tipo inesperado não deve impedir a abertura do programa.
            return false;
        }
    }


    public static bool IsApplied(RegistryOperation operation)
    {
        try
        {
            var (hive, subKey) = SplitPath(operation.Path);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey, writable: false);

            return operation.OperationType switch
            {
                "CreateKey" => key is not null,
                "DeleteKey" => key is null,
                _ => false
            };
        }
        catch
        {
            return false;
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

        return payload
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Convert.ToByte(x.Trim(), 16))
            .ToArray();
    }
}
