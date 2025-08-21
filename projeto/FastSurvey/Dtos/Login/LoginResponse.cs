namespace FASTSURVEY.Dtos.Auth
{
    // Record posicional para resposta do login
    public record LoginResponse(
        int Id,
        string Usuario,
        int TipoUsuarioId,
        string Token
    );

    // Record posicional para resposta do cadastro
    public record CadastrarLoginResponse(
        int Id,
        string Usuario,
        string Email,
        int TipoUsuarioId
    );
}

