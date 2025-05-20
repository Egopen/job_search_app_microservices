using System.ComponentModel.DataAnnotations;

namespace AuthService.UI.DTO.RequestJSON
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
