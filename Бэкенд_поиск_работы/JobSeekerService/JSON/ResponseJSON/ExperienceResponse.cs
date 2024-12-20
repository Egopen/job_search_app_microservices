using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.JSON.ResponseJSON
{
    public class ExperienceResponse
    {
        public int Id { get; set; } 
        public DateOnly Start_d { get; set; }
        public DateOnly Finish_d { get; set; }
        public string Company_name { get; set; }
        public string Job_name { get; set; }
        public string City { get; set; }
        public string? Desc { get; set; }
        public int ResumeId { get; set; }

    }
}
