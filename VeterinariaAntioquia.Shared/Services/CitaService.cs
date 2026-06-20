namespace VeterinariaAntioquia.Shared.Services;

// ═══════════════════════════════════════════════════════════════
//  CitaService.cs — Servicio de acceso a datos del módulo Citas
//
//  Responsabilidades:
//    · Encapsular todas las llamadas HTTP al API de citas.
//    · Devolver tuplas (data, error) en lugar de lanzar excepciones,
//      para que los componentes Razor puedan mostrar mensajes
//      amigables al usuario sin try/catch en la UI.
//    · El token Bearer se inyecta automáticamente en cada request
//      a través del AuthTokenHandler registrado en Program.cs —
//      este servicio no maneja autenticación directamente.
//
//  Dependencias inyectadas:
//    · HttpClient — configurado con BaseAddress y AuthTokenHandler
//      vía IHttpClientFactory en Program.cs / MauiProgram.cs
// ═══════════════════════════════════════════════════════════════

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VeterinariaAntioquia.Shared.Models;

public class CitaService
{
    // ── Opciones de deserialización compartidas ────────────────
    // PropertyNameCaseInsensitive: acepta tanto camelCase (JS/Node)
    // como PascalCase (.NET) en las respuestas del API.
    private readonly JsonSerializerOptions _jsonOpts = new() 
    { 
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString 
    };

    private readonly HttpClient _http;

    /// <param name="http">
    /// HttpClient preconfigurado con BaseAddress y el pipeline
    /// de AuthTokenHandler. Inyectado por el contenedor de DI.
    /// </param>
    public CitaService(HttpClient http)
    {
        _http = http;
    }

    // ═══════════════════════════════════════════════════════════
    //  GET /api/mascotas/mias
    //
    //  Requiere: Bearer token (extrae el id_usuario del JWT)
    //
    //  Devuelve: lista de mascotas pertenecientes al usuario
    //            autenticado, independientemente de su rol.
    // ═══════════════════════════════════════════════════════════
    public async Task<(List<Mascota> data, string? error)> GetMisMascotasAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/mascotas/mias");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (new(), "Sesión expirada. Vuelve a iniciar sesión.");

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CitaService.GetMisMascotas] HTTP {(int)response.StatusCode}: {body}");
                return (new(), "No se pudieron cargar las mascotas.");
            }

            var result = JsonSerializer.Deserialize<List<Mascota>>(body, _jsonOpts);
            return (result ?? new(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.GetMisMascotas] EXCEPTION: {ex.Message}");
            return (new(), "Error de conexión al cargar mascotas.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  GET /api/usuarios/veterinarios
    //
    //  Requiere: Bearer token
    //
    //  Devuelve: lista de usuarios con rol = 'veterinario'
    //            y activo = 1, ordenados por nombre ASC.
    //
    // ═══════════════════════════════════════════════════════════
    public async Task<(List<Veterinario> data, string? error)> GetVeterinariosAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/usuarios/veterinarios");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (new(), "Sesión expirada. Vuelve a iniciar sesión.");

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CitaService.GetVeterinarios] HTTP {(int)response.StatusCode}: {body}");
                return (new(), "No se pudieron cargar los veterinarios.");
            }

            var result = JsonSerializer.Deserialize<List<Veterinario>>(body, _jsonOpts);
            return (result ?? new(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.GetVeterinarios] EXCEPTION: {ex.Message}");
            return (new(), "Error de conexión al cargar veterinarios.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  GET /api/servicios
    //
    //  Requiere: Bearer token
    //
    //  Devuelve: todos los servicios con activo = 1,
    //            ordenados por categoria ASC, nombre ASC.
    //
    // ═══════════════════════════════════════════════════════════
    public async Task<(List<Servicio> data, string? error)> GetServiciosAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/servicios");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (new(), "Sesión expirada. Vuelve a iniciar sesión.");

            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[CitaService.GetServicios] HTTP {(int)response.StatusCode}");
            Console.WriteLine($"[CitaService.GetServicios] Body: {body}");

            if (!response.IsSuccessStatusCode)
                return (new(), $"Error {(int)response.StatusCode} al cargar servicios.");

            var result = JsonSerializer.Deserialize<List<Servicio>>(body, _jsonOpts);
            return (result ?? new(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.GetServicios] EXCEPTION: {ex.GetType().Name} — {ex.Message}");
            return (new(), $"Error de conexión al cargar servicios.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  GET /api/citas/disponibilidad?veterinarioId={id}&fecha={YYYY-MM-DD}
    //
    //  Requiere: Bearer token
    //
    //  Devuelve: lista de slots ya ocupados para el veterinario
    //            en la fecha indicada. El cliente usa estos slots
    //            para deshabilitar botones de hora en el wizard.
    // ═══════════════════════════════════════════════════════════
    public async Task<(List<SlotOcupado> data, string? error)> GetDisponibilidadAsync(
        int veterinarioId,
        DateTime fecha)
    {
        try
        {
            var url = $"api/citas/disponibilidad?veterinarioId={veterinarioId}&fecha={fecha:yyyy-MM-dd}";
            var response = await _http.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (new(), "Sesión expirada. Vuelve a iniciar sesión.");

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CitaService.GetDisponibilidad] HTTP {(int)response.StatusCode}: {body}");
                return (new(), "No se pudo verificar la disponibilidad.");
            }

            var result = JsonSerializer.Deserialize<List<SlotOcupado>>(body, _jsonOpts);
            return (result ?? new(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.GetDisponibilidad] EXCEPTION: {ex.Message}");
            return (new(), "Error de conexión al verificar disponibilidad.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  GET /api/citas/mias
    //
    //  Requiere: Bearer token (extrae el id_usuario del JWT)
    //
    //  Devuelve: todas las citas del usuario autenticado,
    //            enriquecidas con datos de mascota, veterinario
    //            y servicio (JOIN en el API).
    //            Ordenadas por fecha DESC (más reciente primero).
    //
    // ═══════════════════════════════════════════════════════════
    public async Task<(List<CitaResumen> data, string? error)> GetMisCitasAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/citas/mias");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (new(), "Sesión expirada. Vuelve a iniciar sesión.");

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CitaService.GetMisCitas] HTTP {(int)response.StatusCode}: {body}");
                return (new(), "No se pudieron cargar tus citas.");
            }

            var result = JsonSerializer.Deserialize<List<CitaResumen>>(body, _jsonOpts);
            return (result ?? new(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.GetMisCitas] EXCEPTION: {ex.Message}");
            return (new(), "Error de conexión al cargar las citas.");
        }
    }

    public async Task<(CrearCitaResponse? cita, string? error)> CrearCitaAsync(
        CrearCitaRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/citas", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, "Sesión expirada. Vuelve a iniciar sesión.");

            if (response.StatusCode == HttpStatusCode.Conflict)
                return (null, "El veterinario ya tiene una cita en ese horario.");

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return (null, "Datos inválidos. Revisa los campos e intenta de nuevo.");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CitaService.CrearCita] HTTP {(int)response.StatusCode}: {body}");
                return (null, "No se pudo crear la cita.");
            }

            var result = await response.Content.ReadFromJsonAsync<CrearCitaResponse>(_jsonOpts);
            return (result, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.CrearCita] EXCEPTION: {ex.Message}");
            return (null, "Error de conexión al crear la cita.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PATCH /api/citas/{id}/cancelar
    //
    //  Requiere: Bearer token
    //            El API debe validar que la cita pertenece al
    //            usuario autenticado antes de cancelar.
    //
    //  Body (CancelarCitaRequest):
    //  {
    //    "motivo": "No puedo asistir"   // nullable
    //  }
    public async Task<string?> CancelarCitaAsync(int citaId, string? motivo = null)
    {
        try
        {
            var request = new CancelarCitaRequest { Motivo = motivo };
            var response = await _http.PatchAsJsonAsync($"api/citas/{citaId}/cancelar", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return "Sesión expirada. Vuelve a iniciar sesión.";

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return "No tienes permiso para cancelar esta cita.";

            if (response.StatusCode == HttpStatusCode.NotFound)
                return "La cita no existe.";

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return "Esta cita no puede cancelarse en su estado actual.";

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CitaService.CancelarCita] HTTP {(int)response.StatusCode}");
                return "No se pudo cancelar la cita.";
            }

            return null; // null = éxito
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CitaService.CancelarCita] EXCEPTION: {ex.Message}");
            return "Error de conexión al cancelar la cita.";
        }
    }
}