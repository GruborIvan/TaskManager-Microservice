using System;
using System.Threading;
using System.Threading.Tasks;
using FiveDegrees.Messages.Task;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace TaskManager.BackgroundWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBus _bus;

        public Worker(ILogger<Worker> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.Subscribe<CreateTaskMsg>();
            await _bus.Subscribe<CreateTaskMsgV2>();
            await _bus.Subscribe<CreateTaskMsgV3>();
            await _bus.Subscribe<UpdateTaskMsg>();
            await _bus.Subscribe<UpdateTaskMsgV2>();
            await _bus.Subscribe<AssignTaskToEntityMsg>();
            await _bus.Subscribe<AssignTaskToEntityMsgV2>();
            await _bus.Subscribe<RelateTaskToEntityMsg>();
            await _bus.Subscribe<RelateTaskToEntityMsgV2>();
            await _bus.Subscribe<RelateTaskToEntityMsgV3>();
            await _bus.Subscribe<UnassignTaskMsg>();
            await _bus.Subscribe<UnassignTaskMsgV2>();
            await _bus.Subscribe<UpdateTaskDataMsg>();
            await _bus.Subscribe<UpdateTaskDataMsgV2>();
            await _bus.Subscribe<UpdateTaskStatusMsg>();
            await _bus.Subscribe<UpdateTaskStatusMsgV2>();
            await _bus.Subscribe<StoreCommentMsg>();
            await _bus.Subscribe<StoreCommentMsgV2>();
            await _bus.Subscribe<FinalizeTaskStatusMsg>();
            await _bus.Subscribe<FinalizeTaskStatusMsgV2>();
            await _bus.Subscribe<ReportingTaskMsg>();

            _logger.Log(LogLevel.Information, $"Task message listener service started at {DateTime.UtcNow}");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(120000, stoppingToken);
            }

            await _bus.Unsubscribe<CreateTaskMsg>();
            await _bus.Unsubscribe<CreateTaskMsgV2>();
            await _bus.Unsubscribe<CreateTaskMsgV3>();
            await _bus.Unsubscribe<UpdateTaskMsg>();
            await _bus.Unsubscribe<UpdateTaskMsgV2>();
            await _bus.Unsubscribe<AssignTaskToEntityMsg>();
            await _bus.Unsubscribe<AssignTaskToEntityMsgV2>();
            await _bus.Unsubscribe<RelateTaskToEntityMsg>();
            await _bus.Unsubscribe<RelateTaskToEntityMsgV2>();
            await _bus.Unsubscribe<RelateTaskToEntityMsgV3>();
            await _bus.Unsubscribe<UnassignTaskMsg>();
            await _bus.Unsubscribe<UnassignTaskMsgV2>();
            await _bus.Unsubscribe<UpdateTaskDataMsg>();
            await _bus.Unsubscribe<UpdateTaskDataMsgV2>();
            await _bus.Unsubscribe<UpdateTaskStatusMsg>();
            await _bus.Unsubscribe<UpdateTaskStatusMsgV2>();
            await _bus.Unsubscribe<StoreCommentMsg>();
            await _bus.Unsubscribe<StoreCommentMsgV2>();
            await _bus.Unsubscribe<FinalizeTaskStatusMsg>();
            await _bus.Unsubscribe<FinalizeTaskStatusMsgV2>();
            await _bus.Unsubscribe<ReportingTaskMsg>();
        }
    }
}
