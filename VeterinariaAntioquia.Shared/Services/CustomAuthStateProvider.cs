namespace VeterinariaAntioquia.Shared.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IStorageService _storage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(IStorageService storage, HttpClient http)
    {
        _storage = storage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsync("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Configuramos el token en las cabeceras de HttpClient para futuras peticiones
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }
    
    // Agrégalo en tu CustomAuthStateProvider.cs
    public void NotifyUserLogout()
    {
        // 1. Creamos una "Identidad" vacía (lo que Blazor entiende como un usuario anónimo)
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
    
        // 2. Creamos el nuevo estado de autenticación con este usuario fantasma
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
    
        // 3. Le gritamos a toda la aplicación Blazor: "¡El usuario ya no existe, oculten las cosas privadas!"
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            var value = kvp.Value.ToString();
            if (kvp.Key == "nombre")
            {
                claims.Add(new Claim(ClaimTypes.Name, value));
            }
            if (kvp.Key == "rol")
            {
                claims.Add(new Claim(ClaimTypes.Role, value));
            }

            claims.Add(new Claim(kvp.Key, value));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}