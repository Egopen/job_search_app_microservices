using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.JSON.ResponseJSON
{
    public class StatusResponse
    {
        public int Id { get; set; }
        public bool Is_active { get; set; }
        public string? Desc { get; set; }
    }
}
