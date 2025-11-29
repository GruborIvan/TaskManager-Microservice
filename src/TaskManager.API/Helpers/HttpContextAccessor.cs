using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.API.Helpers
{
    public class HttpContextAccessor : IContextAccessor
    {
        private const string _requestIdHeaderKey = "x-request-id";
        private const string _commandIdHeaderKey = "x-command-id";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void CheckIfCommandIdAndRequestIdExists()
        {
            var headers = _httpContextAccessor.HttpContext.Request.Headers;
            _ = headers.FirstOrDefault(x => x.Key.Equals(_commandIdHeaderKey, StringComparison.OrdinalIgnoreCase)).Value.ToString()
                ?? throw new MissingCommandIdException();
            _ = headers.FirstOrDefault(x => x.Key.Equals(_requestIdHeaderKey, StringComparison.OrdinalIgnoreCase)).Value.ToString()
                ?? throw new MissingRequestIdException();
        }

        public Guid GetCommandId()
        {
            return (Guid.TryParse
                    (_httpContextAccessor.HttpContext.Request.Headers
                    .FirstOrDefault(hc => hc.Key == _commandIdHeaderKey).Value, out var commandId))
                ? commandId
                : Guid.Empty;
        }

        public Guid GetCorrelationId()
        {
            return Guid.TryParse
                    (_httpContextAccessor.HttpContext.Request.Headers
                    .FirstOrDefault(hc => string.Equals(hc.Key, _requestIdHeaderKey, StringComparison.OrdinalIgnoreCase)).Value, out var correlationId)
                ? correlationId
                : Guid.Empty;
        }

        public Guid GetRequestId()
        {
            return Guid.TryParse
                    (_httpContextAccessor.HttpContext.Request.Headers
                    .FirstOrDefault(hc => string.Equals(hc.Key,_requestIdHeaderKey, StringComparison.OrdinalIgnoreCase)).Value, out var requestId)
                ? requestId
                : Guid.Empty;
        }
    }
}
