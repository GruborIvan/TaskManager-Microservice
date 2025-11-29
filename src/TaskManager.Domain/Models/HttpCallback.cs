using System;

namespace TaskManager.Domain.Models
{
    public class HttpCallback : Callback
    {
        public Uri Url { get; }

        public HttpCallback(Uri url)
        {
            Url = url;
            Parameters = url?.ToString();
        }
    }
}
