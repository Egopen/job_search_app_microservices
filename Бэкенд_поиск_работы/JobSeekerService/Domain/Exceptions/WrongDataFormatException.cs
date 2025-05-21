namespace JobSeekerService.Domain.Exceptions
{
    public class WrongDataFormatException:Exception
    {
        public WrongDataFormatException(string message) : base(message) { }
        public WrongDataFormatException() { }
    }
}
