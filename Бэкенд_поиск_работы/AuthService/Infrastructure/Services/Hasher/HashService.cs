using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Services.Hasher
{
    public class HashService : IHashService
    {
        public string CreateHash(string input)
        {
            var byteInp = Encoding.ASCII.GetBytes(input);
            var hashInp = MD5.HashData(byteInp);
            StringBuilder sOutput = new StringBuilder(hashInp.Length);
            for (int i = 0; i < hashInp.Length; i++)
            {
                sOutput.Append(hashInp[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}
