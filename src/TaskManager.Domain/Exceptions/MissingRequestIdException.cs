using System;

namespace TaskManager.Domain.Exceptions
{
    [Serializable]
    public class MissingRequestIdException : Exception
    {
        public MissingRequestIdException() : base("Missing x-request-id in headers")
        {

        }
    }
}
