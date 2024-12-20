using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.JSON.RequestJSON
{
    public class UpdateResumeRequest
    {
        [Required]
        public int Id { get; set; }
        public string? Desc { get; set; }
        public string? Mobile_phone { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Job_name { get; set; }
        public int Salary { get; set; }
        public int StatusId { get; set; }
    }
}
