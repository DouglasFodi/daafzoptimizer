using System.Text.Json;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class CatalogService
{
    public static List<Tweak> Load()
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Data", "tweaks.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Tweak>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Tweak>();
    }
}
