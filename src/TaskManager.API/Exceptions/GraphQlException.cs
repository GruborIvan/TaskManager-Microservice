using System;

namespace TaskManager.API.Exceptions
{
    public class GraphQlException : Exception
    {
        public GraphQlException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
