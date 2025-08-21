using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Auth
{
    public class LoginRequest(string usuario, string senha)
    {
        [Required] public string Usuario { get; init; } = usuario;
        [Required] public string Senha { get; init; } = senha;
    }
    // cadastro via multipart; se preferir JSON, criamos um record e um [FromBody]
    public class CadastrarLoginRequest
    {
        [FromForm(Name = "usuario")] public string Usuario { get; set; } = default!;
        [FromForm(Name = "senha")] public string Senha { get; set; } = default!;
        [FromForm(Name = "email")] public string Email { get; set; } = default!;
    }
}
