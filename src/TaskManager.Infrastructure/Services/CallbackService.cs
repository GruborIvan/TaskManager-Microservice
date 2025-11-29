using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure.Services
{
    public class CallbackService : ICallbackService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CallbackService> _logger;

        public CallbackService(IHttpClientFactory client, ILogger<CallbackService> logger)
        {
            _logger = logger;
            _httpClient = client.CreateClient();
        }

        public async System.Threading.Tasks.Task Callback(Callback callback, Task task)
        {
            var result = await _httpClient.PostAsync(
                (callback as HttpCallback).Url.AbsoluteUri, 
                new StringContent(JsonConvert.SerializeObject(new { task.Status, task.TaskId, task.Data })));

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"Unsuccessful callback for task {task.TaskId}, status code: {result.StatusCode}");
            }
        }
    }
}
