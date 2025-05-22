using Serilog;

namespace EmployerService.Infrastructure.Features.Logger
{
    public class LoggerService : ILoggerService
    {
        public LoggerService()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void LogDebug(string message)
        {
            Log.Debug(message);
        }

        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        public void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public void LogError(string message, Exception ex = null)
        {
            if (ex == null)
            {
                Log.Error(message);
            }
            else
            {
                Log.Error(ex, message);
            }
        }

        public void LogFatal(string message, Exception ex = null)
        {
            if (ex == null)
            {
                Log.Fatal(message);
            }
            else
            {
                Log.Fatal(ex, message);
            }
        }
    }
}
