namespace JobSeekerService.Infrastructure.DB.Models
{
    public class Status
    {
        public int Id { get; set; }

        public bool Is_active { get; set; }
        public string Desc { get; set; }
        List<Resume> Resumes { get; set; } = new();
    }
}
