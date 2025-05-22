using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;

namespace JobSeekerService.Infrastructure.Features
{
    public class HashManager
    {
        public static string CreateHash(string input)
        {
            var byteInp = Encoding.ASCII.GetBytes(input);
            var hashInp = MD5.HashData(byteInp);
            int i;
            StringBuilder sOutput = new StringBuilder(hashInp.Length);
            for (i = 0; i < hashInp.Length; i++)
            {
                sOutput.Append(hashInp[i].ToString("X2"));
            }
            return sOutput.ToString();

        }
    }
}
