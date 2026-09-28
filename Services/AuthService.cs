using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using RegOptimizer.Models;

namespace RegOptimizer.Services;

public static class AuthService
{
    public static async Task<AuthResponse> AuthenticateAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            return new AuthResponse { Ok = false, Message = "Informe a senha." };

        var config = AuthConfigService.Load();
        var configError = AuthConfigService.Validate(config);
        if (configError is not null)
            return new AuthResponse { Ok = false, Message = configError };

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("RegOptimizer/1.0");

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        var payload = new
        {
            password,
            application = config.ApplicationId,
            version
        };

        try
        {
            using var response = await client.PostAsJsonAsync(config.Endpoint, payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            AuthResponse? parsed = null;
            try
            {
                parsed = JsonSerializer.Deserialize<AuthResponse>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                // A resposta pública para o usuário permanece genérica abaixo.
            }

            if (response.IsSuccessStatusCode && parsed?.Ok == true)
                return new AuthResponse { Ok = true, Message = parsed.Message };

            if ((int)response.StatusCode == 429)
                return new AuthResponse { Ok = false, Message = parsed?.Message ?? "Muitas tentativas. Tente novamente mais tarde." };

            return new AuthResponse { Ok = false, Message = parsed?.Message ?? "Senha inválida ou acesso recusado." };
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AuthResponse { Ok = false, Message = "O servidor de autenticação não respondeu dentro do tempo limite." };
        }
        catch (HttpRequestException)
        {
            return new AuthResponse { Ok = false, Message = "Não foi possível conectar ao servidor de autenticação." };
        }
        catch
        {
            return new AuthResponse { Ok = false, Message = "Falha inesperada durante a autenticação." };
        }
    }
}
