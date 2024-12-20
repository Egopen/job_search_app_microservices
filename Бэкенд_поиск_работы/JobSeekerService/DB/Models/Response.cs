using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeekerService.DB.Models
{
    public class Response
    {
        public int Id { get; set; }

        public int Vacancy_id { get; set; }
        [Column("Resume_id")]
        public int ResumeId { get; set; }
        public Resume? Resume { get; set; }


    }
}
