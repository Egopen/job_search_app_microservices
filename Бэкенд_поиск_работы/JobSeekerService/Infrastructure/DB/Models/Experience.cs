using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeekerService.Infrastructure.DB.Models
{
    public class Experience
    {
        public int Id { get; set; }
        public DateOnly Start_d { get; set; }
        public DateOnly Finish_d { get; set; }
        public string Company_name { get; set; }
        public string Job_name { get; set; }
        public string City { get; set; }
        public string? Desc { get; set; }
        [Column("Resume_id")]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }
    }
}
