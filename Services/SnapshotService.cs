using System.Text.Json;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class SnapshotService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static string DirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "RegOptimizer", "snapshots");

    public static string Save(RegistrySnapshot snapshot)
    {
        Directory.CreateDirectory(DirectoryPath);
        var file = Path.Combine(DirectoryPath, $"snapshot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json");
        File.WriteAllText(file, JsonSerializer.Serialize(snapshot, Options));
        File.WriteAllText(Path.Combine(DirectoryPath, "latest.txt"), file);
        return file;
    }

    public static RegistrySnapshot? FindLatestContaining(
        IEnumerable<string> tweakIds,
        IEnumerable<string>? operationIds = null)
    {
        Directory.CreateDirectory(DirectoryPath);
        var neededTweaks = tweakIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var neededOperations = (operationIds ?? Array.Empty<string>())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (neededTweaks.Count == 0 && neededOperations.Count == 0) return null;

        foreach (var file in Directory.EnumerateFiles(DirectoryPath, "snapshot-*.json")
                                      .OrderByDescending(File.GetLastWriteTimeUtc))
        {
            try
            {
                var snap = JsonSerializer.Deserialize<RegistrySnapshot>(File.ReadAllText(file), Options);
                if (snap is null) continue;

                var presentTweaks = snap.Items.Select(x => x.TweakId)
                                              .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var presentOperations = snap.Operations.Select(x => x.OperationId)
                                                       .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (neededTweaks.All(presentTweaks.Contains) &&
                    neededOperations.All(presentOperations.Contains))
                    return snap;
            }
            catch
            {
                // Ignore malformed/partial historical snapshots and keep looking.
            }
        }
        return null;
    }
}
