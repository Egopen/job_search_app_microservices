namespace StatisticService.DB.Models
{
    public class VacancyStatistic
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public int Active { get; set; }
        public string Active_part { get; set; }
        public int Inactive { get; set; }
        public string Inactive_part { get; set; }
        public DateTime Date_of_record { get; set; }
    }
}
