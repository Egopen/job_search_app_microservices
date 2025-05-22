namespace StatisticService.Domain.JSON.RabbitMQClasses
{
    public class VacancyStatisticJSON
    {
        public int Total { get; set; }
        public int Inactive { get; set; }
        public int Active { get; set; }
        public DateTime DateOfRecord { get; set; }


    }
}
