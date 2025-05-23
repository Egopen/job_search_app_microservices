﻿using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JobSeekerService.Infrastructure.Settings
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        static string KEY = Environment.GetEnvironmentVariable("KEY");
        public static SymmetricSecurityKey GetSymSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
