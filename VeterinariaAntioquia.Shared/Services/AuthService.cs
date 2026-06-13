namespace VeterinariaAntioquia.Shared.Services;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using VeterinariaAntioquia.Shared.Models;
public class AuthService
{
    private readonly HttpClient _http;
    private readonly IStorageService _storage;
    private readonly AuthenticationStateProvider _authStateProvider;
    
    public AuthService(HttpClient http, IStorageService storage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _storage = storage;
        _authStateProvider = authStateProvider;
    }
    // Guardar el token en el almacenamiento local
    public async void SetAuthToken(string token)
    {
        await _storage.SaveItemAsync("authToken", token);
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);
    }
    // Funcion para registrar un usuario
    public async Task<string?> Register(string nombre, string email, string password)
    {
        try
        {
            var request = new RegisterRequest { Nombre = nombre, Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("api/auth/register", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                if (result != null)
                {
                    SetAuthToken(result.Token);
                    return null; //Registro exitoso, no hay error
                }
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) return "Todos los campos son obligatorios";
            return "Error al conectar con el servidor";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ERROR: {ex.Message}");
            return ex.Message;
        }
        
    }
    public async Task<string?> Login(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };

            var response = await _http.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    SetAuthToken(result.Token);
                    return null; // Login exitoso, no hay error
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return "Credenciales incorrectas";
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return "Esta cuenta ha sido desactivada";

            return "Error al conectar con el servidor";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ERROR: {ex.Message}");
            return ex.Message;
        }
    }
    // Funcion cerrar sesion
    public async Task Logout()
    {
        await _storage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }
}