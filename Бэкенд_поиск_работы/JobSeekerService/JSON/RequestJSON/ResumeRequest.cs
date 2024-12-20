using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.JSON.RequestJSON
{
    public class ResumeRequest
    {
        public string Desc { get; set; } = "";
        [Required]
        public string Name { get; set; }
        [Required]
        public string MobilePhone { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public int StatusId { get; set; }
        [Required]
        public string JobName { get; set; }
        [Required]
        public uint Salary {  get; set; }

    }
}
