using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeekerService.JSON.RequestJSON
{
    public class ExperienceRequest
    {
        [Required]
        public DateOnly Start_d { get; set; }
        [Required]
        public DateOnly Finish_d { get; set; }
        [Required]
        public string Company_name { get; set; }
        [Required]
        public string Job_name { get; set; }
        [Required]
        public string City { get; set; }
        public string? Desc { get; set; }
        [Required]
        public int ResumeId { get; set; }
    }
}
