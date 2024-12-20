using System.ComponentModel.DataAnnotations;

namespace EmployerService.JSON.RequestJSON
{
    public class UpdateVacancyRequest
    {
        [Required]
        public int Id { get; set; } 
        public string? Job_name { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public int Status_id { get; set; }
        public int Experience_id { get; set; }
    }
}
