using System.Security.Cryptography;
using System.Text;

namespace ClothingStore
{
    public static class SecurityHelper
    {
        /// <summary>Returns the SHA-256 hex hash of the given plain-text password.</summary>
        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
