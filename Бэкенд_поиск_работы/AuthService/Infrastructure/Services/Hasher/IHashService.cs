namespace AuthService.Infrastructure.Services.Hasher
{
    public interface IHashService
    {
        string CreateHash(string input);
    }
}
