#nullable enable
using System;

namespace FASTSURVEY.Dtos.Tokens
{
    public class TokenDto
    {
        public int TokenId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime DataRegistro { get; set; }
        public DateTime DataExpirado { get; set; }
        public bool Ativo => DataExpirado > DateTime.UtcNow;
    }

    public class CriarTokenRequest
    {
        /// <summary>Validade em minutos (default: 10)</summary>
        public int? ValidadeMinutos { get; set; }
    }

    public class ProrrogarTokenRequest
    {
        /// <summary>Minutos a somar na validade</summary>
        public int Minutos { get; set; }
    }

    public class ValidarTokenResponse
    {
        public bool Valido { get; set; }
        public int? TokenId { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public string? MotivoInvalidez { get; set; }
    }
}
