namespace RegOptimizer.Models;

public sealed class AuthConfig
{
    public string Endpoint { get; set; } = "";
    public string ApplicationId { get; set; } = "RegOptimizer";
    public int TimeoutSeconds { get; set; } = 12;
    public bool RequireHttps { get; set; } = true;
}

public sealed class AuthResponse
{
    public bool Ok { get; set; }
    public string Message { get; set; } = "";
}
