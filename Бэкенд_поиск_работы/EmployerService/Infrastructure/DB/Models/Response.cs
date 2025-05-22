using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.Infrastructure.DB.Models
{
    public class Response
    {
        public int Id { get; set; }
        public int Resume_id { get; set; }
        [Column("Vacancy_id")]
        public int VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }
    }
}
