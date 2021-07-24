using System.Security.Cryptography;
using System.Text;

namespace LXGaming.Ticket.Server.Util {

    public static class HashUtils {

        public static string CreateSha256(string input) {
            using var algorithm = SHA256.Create();
            return Create(algorithm, input);
        }

        private static string Create(HashAlgorithm algorithm, string input) {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = algorithm.ComputeHash(inputBytes);

            var stringBuilder = new StringBuilder();
            for (var index = 0; index < hashBytes.Length; index++) {
                stringBuilder.Append(hashBytes[index].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}