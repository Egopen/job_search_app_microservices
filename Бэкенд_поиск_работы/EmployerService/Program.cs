
using EmployerService.DB.DBContext;
using EmployerService.Features;
using EmployerService.Features.Logger;
using EmployerService.Options;
using EmployerService.RabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RabbitMQInitializer;

namespace EmployerService
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

            builder.Services.AddDbContext<Context>(db => db.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
            builder.Services.AddSingleton<LoggerService>();
            builder.Services.AddSingleton<RabbitMQService>();
            builder.Services.AddHostedService<RabbitMQAddResponseBackgroundService>();
            builder.Services.AddHostedService<RabbitMQSendStatisticService>();
            ///
            /// Authorization
            ///
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
                    Title = "Employer Service API",
                    Version = "v1",
                    Description = "API для управления вакансиями."
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
                options.ListenAnyIP(8082);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            app.Run();
        }
    }
}
