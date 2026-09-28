using System.Text.Json;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class SmartCatalogService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<SmartOption> LoadOptions(IReadOnlyDictionary<string, Tweak> tweaks)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "smart_options.json");
        var items = JsonSerializer.Deserialize<List<SmartOption>>(File.ReadAllText(path), Options)
                    ?? new List<SmartOption>();

        foreach (var option in items)
            option.Items = option.TweakIds.Where(tweaks.ContainsKey).Select(id => tweaks[id]).ToList();

        return items;
    }

    public static List<SmartProfile> LoadProfiles()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "profiles.json");
        return JsonSerializer.Deserialize<List<SmartProfile>>(File.ReadAllText(path), Options)
               ?? new List<SmartProfile>();
    }
}
