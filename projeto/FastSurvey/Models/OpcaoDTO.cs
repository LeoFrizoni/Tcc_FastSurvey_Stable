namespace FASTSURVEY.Models
{
    public class OpcaoDto
    {
        public string Texto { get; set; } = string.Empty;
        // Quando tem gabarito, marcamos quais são corretas (true). Se sem gabarito, manter false.
        public bool Correta { get; set; } = false;
    }
}
