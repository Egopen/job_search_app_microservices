namespace MVCFrontForJobSeek.JSON
{
    public class ResumeDetails
    {
        public int Id { get; set; }
        public string? Desc { get; set; }
        public string Name { get; set; }
        public string? Mobile_phone { get; set; }
        public string? City { get; set; }
        public string Job_name { get; set; }
        public int Salary { get; set; }
        public int Job_seekerId { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string Experience { get; set; }
    }
}
