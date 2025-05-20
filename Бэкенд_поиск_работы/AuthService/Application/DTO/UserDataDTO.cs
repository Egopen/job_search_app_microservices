namespace AuthService.Application.DTO
{
    public class UserDataDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string RefreshToken {  get; set; }
    }
}
