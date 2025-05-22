namespace EmployerService.Infrastructure.Features.Logger
{
    public interface ILoggerService
    {
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogFatal(string message, Exception ex = null);
    }
}
