using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.DB.Models
{
    public class Vacancy
    {
        public int Id { get; set; }
        public string Job_name { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        [Column("Employer_id")]
        public int EmployerId { get; set; }
        [Column("Status_id")]
        public int StatusId { get; set; }
        public Status Status { get; set; }
        [Column("Experience_id")]
        public int ExperienceId { get; set; }
        public Experience Experience { get; set; }
        List<Response> Response { get; set; } = new();
    }
}
