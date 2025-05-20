namespace AuthService.Application.Exceptions
{
    public class UserExistsExcpetion:Exception
    {
        public UserExistsExcpetion(string message) : base(message)
        { }
        public UserExistsExcpetion() : base()
        { }
    }
}
