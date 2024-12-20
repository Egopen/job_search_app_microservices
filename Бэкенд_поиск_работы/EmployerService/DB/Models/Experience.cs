using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.DB.Models
{
    public class Experience
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public List<Vacancy> Vacancy { get; set; }
    }
}
