using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class CreateReportHandlerTests
    {
        private readonly Mock<CreateReportValidator> _mockValidator = new Mock<CreateReportValidator>();
        private readonly Mock<IReportingService> _mockReportingService = new Mock<IReportingService>();

        public CreateReportHandlerTests()
        {
            _mockValidator.Setup(v => v.ValidateAndThrow(It.IsAny<CreateReport>()))
                .Verifiable();

            _mockReportingService
                .Setup(m => m.GetReportingDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, byte[]>());
        }

        [Fact]
        public async Task ValidCommand_CreateReport_Succeeds()
        {
            var command = new CreateReport(
                correlationId: Guid.NewGuid(),
                dboEntities: new List<string> { "Task", "Comment" },
                fromDatetime: DateTime.UtcNow,
                toDatetime: null,
                initiatedBy: Guid.NewGuid());

            var createReportCommandHandler = new CreateReportHandler(_mockReportingService.Object, _mockValidator.Object);
            await createReportCommandHandler.Handle(command, It.IsAny<CancellationToken>());

            _mockReportingService.Verify(x => x.GetReportingDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockReportingService.Verify(x => x.StoreReportAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, byte[]>>(), It.IsAny<CancellationToken>()), Times.Once());
            _mockValidator.Verify(x => x.ValidateAndThrow(It.IsAny<CreateReport>()), Times.Once());
        }

        [Fact]
        public async Task NullCommand_Throws_ArgumentNullException()
        {
            // Arrange
            CreateReport invalidCommand = null;

            _mockValidator.Setup(v => v.ValidateAndThrow(It.IsAny<CreateReport>()))
                .Throws(new ArgumentNullException());

            var handler = new CreateReportHandler(_mockReportingService.Object, _mockValidator.Object);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(invalidCommand, default));

            _mockValidator.Verify();
        }
    }
}
