namespace AuthService.Application.DTO
{
    public class CreateUserDTO
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string RefreshToken { get; set; }
    }
}
