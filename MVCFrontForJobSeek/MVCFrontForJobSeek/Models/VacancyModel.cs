using System.ComponentModel.DataAnnotations;

namespace MVCFrontForJobSeek.Models
{
    public class VacancyModel
    {
        [Required]
        public string Job_name { get; set; }

        public string? Description { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public int Status_id { get; set; }

        [Required]
        public int Experience_id { get; set; }
    }
}
