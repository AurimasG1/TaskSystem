namespace TaskSystem.Domain.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(int id)
            : base($"Task with id {id} not found.") { }
    }
}
