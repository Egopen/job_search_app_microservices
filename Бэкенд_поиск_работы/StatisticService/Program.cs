
using Microsoft.EntityFrameworkCore;
using StatisticService.Domain.RabbitMQ;
using StatisticService.Infrastructure.DB.DBContext;
using StatisticService.Infrastructure.Features;
using StatisticService.Infrastructure.Features.Logger;
namespace StatisticService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try 
            { EnvReader.Load(".env"); }
            catch (Exception ex){ 
                Console.WriteLine(ex.ToString());
            }
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<Context>(db => db.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
            builder.Services.AddSingleton<LoggerService>();
            builder.Services.AddSingleton<RabbitMQService>();
            builder.Services.AddHostedService<RabbitMQConsumeStatisticService>();
            builder.Services.AddCors();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Employer Service API",
                    Version = "v1",
                    Description = "API для управления вакансиями."
                });


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

                c.OrderActionsBy((apiDesc) => $"{apiDesc.HttpMethod} {apiDesc.RelativePath}");
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8081);
            });
            var app = builder.Build();

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
