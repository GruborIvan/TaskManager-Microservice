namespace TaskManager.Domain.DomainEvents
{
    public class ErrorData
    {
        public ErrorData(string message, string code, string target = "task")
        {
            Message = message;
            Code = code;
            Target = target;
        }

        public string Message { get; }
        public string Code { get; }
        public string Target { get; }
    }
}
