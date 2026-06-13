using Microsoft.AspNetCore.Components.Authorization;
using VeterinariaAntioquia.Web.Components;
using VeterinariaAntioquia.Shared.Services;
using VeterinariaAntioquia.Shared.Services.Styles;
using VeterinariaAntioquia.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the VeterinariaAntioquia.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<CustomToastService>();
// 1. Registro del HttpClient para la Web
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.1.1:3000/") });

// 2. Registro del AuthService
builder.Services.AddScoped<AuthService>();
//Autenticacion
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/404");
//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(VeterinariaAntioquia.Shared._Imports).Assembly);

app.Run();