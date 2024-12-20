using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.JSON.RequestJSON
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
