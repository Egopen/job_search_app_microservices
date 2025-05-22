using JobSeekerService.Domain.RabbitMQ;
using JobSeekerService.Domain.ResponseService;
using JobSeekerService.Domain.ResumeService;
using JobSeekerService.Domain.StatusService;
using JobSeekerService.Infrastructure.DB.DBContext;
using JobSeekerService.Infrastructure.Features;
using JobSeekerService.Infrastructure.Features.Logger;
using JobSeekerService.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JobSeekerService
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
            builder.Services.AddScoped<ILoggerService,LoggerService>();
            builder.Services.AddSingleton<RabbitMQService>();
            builder.Services.AddHostedService<RabbitMQSendResumeStatisticService>();
            builder.Services.AddScoped<IResponseService, ResponseService>();
            builder.Services.AddScoped<IResumeService, ResumeService>();
            builder.Services.AddScoped<IStatusService, StatusService>();
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
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Job Seeker Service API",
                    Version = "v1",
                    Description = "API для управления резюме и другими функциями сервиса."
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
                options.ListenAnyIP(8083); 
            });
            var app = builder.Build();

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
