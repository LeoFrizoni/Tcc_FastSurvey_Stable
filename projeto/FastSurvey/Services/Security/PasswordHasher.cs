using System.Security.Cryptography;

namespace FASTSURVEY.Services.Security
{
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string savedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(savedHash))
                return false;

            // Se n�o for PBKDF2, pode ser uma senha antiga sem hash
            if (!IsPbkdf2(savedHash))
            {
                // Para compatibilidade com senhas antigas (n�o recomendado em produ��o)
                return password.Equals(savedHash, StringComparison.Ordinal);
            }

            var parts = savedHash.Split('$');
            if (parts.Length != 4 || !parts[0].Equals(Prefix, StringComparison.OrdinalIgnoreCase)) 
                return false;

            try
            {
                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var hash = Convert.FromBase64String(parts[3]);

                var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
                return CryptographicOperations.FixedTimeEquals(hash, computed);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPbkdf2(string saved) =>
            !string.IsNullOrWhiteSpace(saved) && saved.StartsWith($"{Prefix}$", StringComparison.OrdinalIgnoreCase);

        // M�todos de compatibilidade
        public static string Hash(string password) => HashPassword(password);
        public static bool Verify(string password, string savedPbkdf2) => VerifyPassword(password, savedPbkdf2);
    }
}
