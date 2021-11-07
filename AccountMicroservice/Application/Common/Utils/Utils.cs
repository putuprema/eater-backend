using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Utils
{
    public static class Utils
    {
        public static string GenerateRandomToken()
        {
            var randomString = Guid.NewGuid().ToString();
            using var alg = SHA256.Create();
            return BitConverter.ToString(alg.ComputeHash(Encoding.UTF8.GetBytes(randomString))).Replace("-", string.Empty);
        }
    }
}
