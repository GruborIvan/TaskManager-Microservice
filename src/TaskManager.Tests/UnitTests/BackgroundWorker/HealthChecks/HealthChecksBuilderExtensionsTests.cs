using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using TaskManager.BackgroundWorker.HealthChecks;

namespace TaskManager.Tests.UnitTests.BackgroundWorker.HealthChecks
{
    public class HealthChecksBuilderExtensionsTests
    {
        private readonly Mock<IHealthChecksBuilder> _mockHealthCheckBuilder = new Mock<IHealthChecksBuilder>();

        [Fact]
        public void IsHealthy()
        {
            //Act
            var result = HealthChecksBuilderExtensions.AddServiceHealthCheck<HealthCheckTest>(_mockHealthCheckBuilder.Object, "serviceName");

            // Assert
            Assert.Null(result);
        }
    }

    public class HealthCheckTest : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

}
