using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.DB.Models
{
    public class Status
    {
        public int Id { get; set; }
        [Column("Is_active")]
        public bool IsActive { get; set; }
        public string? Desc { get; set; }
        List<Vacancy> Vacancy { get; set; } = new();
    }
}
