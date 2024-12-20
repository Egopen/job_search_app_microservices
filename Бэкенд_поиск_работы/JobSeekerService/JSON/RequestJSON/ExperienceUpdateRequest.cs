using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.JSON.RequestJSON
{
    public class ExperienceUpdateRequest
    {
        [Required]
        public int Id { get; set; }
        public DateOnly Start_d { get; set; }
        public DateOnly Finish_d { get; set; }
        public string? Company_name { get; set; }
        public string? Job_name { get; set; }
        public string? City { get; set; }
        public string? Desc { get; set; }
        [Required]
        public int ResumeId { get; set; }
    }
}
