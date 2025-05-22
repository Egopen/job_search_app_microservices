using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeekerService.Infrastructure.DB.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public string? Desc { get; set; }
        public string? Mobile_phone { get; set; }
        public string? City { get; set; }
        public string Name { get; set; }
        public string Job_name { get; set; }
        public int Salary { get; set; }
        [Column("Job_seeker_id")]
        public int Job_seekerId { get; set; }
        [Column("Status_id")]
        public int StatusId { get; set; }

        public Status? Status { get; set; }
        List<Experience> Experience { get; set; } = new();
        List<Response> Response { get; set; } = new();
    }
}
