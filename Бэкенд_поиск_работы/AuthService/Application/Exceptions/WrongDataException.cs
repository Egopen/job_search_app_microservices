namespace AuthService.Application.Exceptions
{
    public class WrongDataException:Exception
    {
        public WrongDataException(string message) : base(message) 
        { }
        public WrongDataException() : base()
        {}
    }
}
