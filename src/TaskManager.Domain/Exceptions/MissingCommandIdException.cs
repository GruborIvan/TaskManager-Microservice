using System;

namespace TaskManager.Domain.Exceptions
{
    [Serializable]
    public class MissingCommandIdException : Exception
    {
        public MissingCommandIdException() : base("Missing x-command-id in headers")
        {

        }
    }
}
