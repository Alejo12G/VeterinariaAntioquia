namespace VeterinariaAntioquia.Shared.Models;
//Clases para el modelo de registro y login

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
public class LoginResponse
{
    public string Token { get; set; }
    public UsuarioDto Usuario { get; set; }
}

public class RegisterResponse
{
    public string Token { get; set; }
    public UsuarioDto Usuario { get; set; }
}
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Rol { get; set; }
}
