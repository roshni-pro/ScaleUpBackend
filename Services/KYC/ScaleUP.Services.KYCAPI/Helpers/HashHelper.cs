using ScaleUP.Services.KYCAPI.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace ScaleUP.Services.KYCAPI.Helpers
{
    public class HashHelper
    {
        public string QuickHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var inputHash = SHA256.HashData(inputBytes);
            return Convert.ToHexString(inputHash);
        }
    }
}
