#nullable enable
using System;

namespace FASTSURVEY.Dtos.Tokens
{
    public class TokenDto
    {
        public int tokenid { get; set; }
        public string token { get; set; } = string.Empty;
        public DateTime dataregistro { get; set; }
        public DateTime dataexpirado { get; set; }
        public bool ativo => dataexpirado > DateTime.UtcNow;
    }

    public class CriarTokenRequest
    {
        /// <summary>Validade em minutos (default: 10)</summary>
        public int? validadeMinutos { get; set; }
    }

    public class ProrrogarTokenRequest
    {
        /// <summary>Minutos a somar na validade</summary>
        public int minutos { get; set; }
    }

    public class ValidarTokenResponse
    {
        public bool valido { get; set; }
        public int? tokenid { get; set; }
        public DateTime? expiraEm { get; set; }
        public string? motivoInvalidez { get; set; }
    }
}
