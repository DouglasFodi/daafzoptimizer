using System.Text.Json;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class AuthConfigService
{
    public static AuthConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "auth.json");
        AuthConfig config = new();

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            config = JsonSerializer.Deserialize<AuthConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AuthConfig();
        }

        var envUrl = Environment.GetEnvironmentVariable("REGOPTIMIZER_AUTH_URL");
        if (!string.IsNullOrWhiteSpace(envUrl))
            config.Endpoint = envUrl.Trim();

        return config;
    }

    public static string? Validate(AuthConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Endpoint) ||
            config.Endpoint.Contains("SEU-DOMINIO", StringComparison.OrdinalIgnoreCase))
            return "Autenticação ainda não foi configurada. Edite Data\\auth.json ou defina REGOPTIMIZER_AUTH_URL.";

        if (!Uri.TryCreate(config.Endpoint, UriKind.Absolute, out var uri))
            return "A URL de autenticação é inválida.";

        var isLocal = uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        if (config.RequireHttps && !isLocal && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return "Por segurança, o endpoint de autenticação deve usar HTTPS.";

        if (config.TimeoutSeconds is < 3 or > 60)
            config.TimeoutSeconds = 12;

        return null;
    }
}
