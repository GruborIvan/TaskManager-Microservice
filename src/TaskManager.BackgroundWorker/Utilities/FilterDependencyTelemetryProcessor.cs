using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TaskManager.BackgroundWorker.Utilities
{
    public class FilterDependencyTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        public FilterDependencyTelemetryProcessor(ITelemetryProcessor next)
        {
            this.next = next;
        }
        public void Process(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;
            if (dependency != null && dependency.Name == "Receive" && dependency.Type == "Azure Service Bus")
            {
                // Exclude this DependencyTelemetry
                return;
            }
            // Pass the telemetry to the next processor in the chain
            this.next.Process(item);
        }
    }

    public class FilterDependencyTelemetryProcessorFactory : Microsoft.ApplicationInsights.WorkerService.ITelemetryProcessorFactory
    {
        public ITelemetryProcessor Create(ITelemetryProcessor nextProcessor)
        {
            return new FilterDependencyTelemetryProcessor(nextProcessor);
        }
    }
}