using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using VeterinariaAntioquia.Shared.Services;
using VeterinariaAntioquia.Shared.Services.Styles;
using VeterinariaAntioquia.Services;

namespace VeterinariaAntioquia;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });
        //Autenticacion
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<IStorageService, SecureStorageService>();
        // 1. Necesitamos registrar el HttpClient para poder hacer peticiones a la API
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.1.1:3000/") });
        // 2. Registramos el AuthService
        builder.Services.AddScoped<AuthService>();
        // Add device-specific services used by the VeterinariaAntioquia.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<CustomToastService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        
        builder.Services.AddMauiBlazorWebView();
        

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}