using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MVCFrontForJobSeek
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ��������� HTTP-������ ��� �������������� � API
            builder.Services.AddHttpClient();

            // ��������� ��������������
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Access", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromHours(2),
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            // ��������� �����������
            builder.Services.AddAuthorization();

            // ��������� ����������� � �������������
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // �������� �������������� � �����������
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Resume}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "vacancy",
                pattern: "Vacancy/{action=Index}/{id?}",
                defaults: new { controller = "Vacancy" });

            app.Run();
        }
    }
}
