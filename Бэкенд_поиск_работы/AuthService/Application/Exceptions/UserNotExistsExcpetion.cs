namespace AuthService.Application.Exceptions
{
    public class UserNotExistsExcpetion:Exception
    {
        public UserNotExistsExcpetion(string message) : base(message)
        { }
        public UserNotExistsExcpetion() : base()
        { }
    }
}
