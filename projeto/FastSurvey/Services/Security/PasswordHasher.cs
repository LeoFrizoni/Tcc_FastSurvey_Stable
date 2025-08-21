using System.Security.Cryptography;

namespace FASTSURVEY.Services.Security
{
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool IsPbkdf2(string saved) =>
            !string.IsNullOrWhiteSpace(saved) && saved.StartsWith($"{Prefix}$", StringComparison.OrdinalIgnoreCase);

        public static bool Verify(string password, string savedPbkdf2)
        {
            var parts = savedPbkdf2.Split('$');
            if (parts.Length != 4 || !parts[0].Equals(Prefix, StringComparison.OrdinalIgnoreCase)) return false;

            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var hash = Convert.FromBase64String(parts[3]);

            var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
            return CryptographicOperations.FixedTimeEquals(hash, computed);
        }
    }
}
