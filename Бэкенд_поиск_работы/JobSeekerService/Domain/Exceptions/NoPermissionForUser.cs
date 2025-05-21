namespace JobSeekerService.Domain.Exceptions
{
    public class NoPermissionForUser:Exception 
    {
        public NoPermissionForUser(string message) : base(message) { }
        public NoPermissionForUser() { }
    }
}
