using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TaskManager.BackgroundWorker.Utilities
{
    public class SamplingConfigurationTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ExceptionTelemetry)
            {
                ((ISupportSampling)telemetry).SamplingPercentage = 100;
            }
        }
    }
}