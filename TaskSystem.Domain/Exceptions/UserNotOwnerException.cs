namespace TaskSystem.Domain.Exceptions
{
    public class UserNotOwnerException : Exception
    {
        public UserNotOwnerException()
            : base("User is not the owner of this task.") { }
    }
}
