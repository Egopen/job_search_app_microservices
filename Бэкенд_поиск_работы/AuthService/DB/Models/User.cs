using Microsoft.EntityFrameworkCore;

namespace AuthService.DB.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Refresh_token { get; set; }
        public string Role { get; set; }
    }
}
