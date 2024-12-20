using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MVCFrontForJobSeek
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        static string KEY = "aasdasdfgdssdasfsdfasadsfdsfsddadsfs";
        public static SymmetricSecurityKey GetSymSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
