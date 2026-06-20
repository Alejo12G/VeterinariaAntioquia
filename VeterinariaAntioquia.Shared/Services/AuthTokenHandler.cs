namespace VeterinariaAntioquia.Shared.Services;

using System.Net.Http.Headers;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IStorageService _storage;

    public AuthTokenHandler(IStorageService storage)
    {
        _storage = storage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Durante el prerendering de Blazor Server, JS Interop no está
            // disponible. Si falla, simplemente enviamos el request sin token.
            var token = await _storage.GetItemAsync("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (InvalidOperationException)
        {
            // JS Interop no disponible en prerender — ignorar y continuar sin token
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}