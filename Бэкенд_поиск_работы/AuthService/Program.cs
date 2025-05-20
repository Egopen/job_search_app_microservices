using AuthService.Application.Services.Auth;
using AuthService.Infrastructure.DB.DBContext;
using AuthService.Infrastructure.Services.DBProxy;
using AuthService.Infrastructure.Services.Hasher;
using AuthService.Infrastructure.Services.Logger;
using AuthService.Infrastructure.Services.Tokens;
using AuthService.Infrastructure.Settings;
using AuthService.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            { EnvReader.Load(".env"); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            var builder = WebApplication.CreateBuilder(args);

            ///
            /// Authorization
            ///
            builder.Services.AddDbContext<Context>(db => db.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
            builder.Services.AddScoped<ILoggerService,LoggerService>();
            builder.Services.AddScoped<IHashService, HashService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserRespository, UserRepository>();
            builder.Services.AddScoped<IAuthService, Application.Services.Auth.AuthService>();
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
            builder.Services.AddAuthentication("Cookies");
            builder.Services.AddAuthorization();

            ///
            /// Authorization
            ///

            builder.Services.AddCors();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Auth Service API",
                    Version = "v1",
                    Description = "API для управления пользователями."
                });


                // Добавление схемы авторизации (JWT)
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Введите JWT токен следующим образом: Bearer {токен}"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Дополнительные настройки (например, сортировка эндпоинтов)
                c.OrderActionsBy((apiDesc) => $"{apiDesc.HttpMethod} {apiDesc.RelativePath}");
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8084);
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }
}
