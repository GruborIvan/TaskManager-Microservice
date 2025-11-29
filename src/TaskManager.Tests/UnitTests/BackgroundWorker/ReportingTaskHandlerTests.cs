using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FiveDegrees.Messages.Task;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using TaskManager.BackgroundWorker.Handlers;
using TaskManager.Domain.Commands;
using Xunit;

namespace TaskManager.Tests.UnitTests.BackgroundWorker
{
    public class ReportingTaskHandlerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IMapper> _autoMapperMock;
        private readonly Mock<ILogger<ReportingTaskHandler>> _loggerMock;
        private readonly Mock<IBus> _busMock = new Mock<IBus>();

        public ReportingTaskHandlerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReport>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            _autoMapperMock = new Mock<IMapper>();
            _autoMapperMock
                .Setup(m => m.Map<CreateReport>(It.IsAny<ReportingTaskMsg>()))
                .Returns(new CreateReport(Guid.NewGuid(), new List<string> { "Task" }, DateTime.Now, null, Guid.NewGuid()))
                .Verifiable();

            _loggerMock = new Mock<ILogger<ReportingTaskHandler>>();
        }

        [Fact]
        public async Task ReportingTask_CreateReportCommandIsSent()
        {
            var message = new ReportingTaskMsg(Guid.NewGuid(), new List<ReportingTaskEntities> { ReportingTaskEntities.Task }, DateTime.Now, null);

            var reportingTaskHandler = new ReportingTaskHandler(_loggerMock.Object, _mediatorMock.Object, _autoMapperMock.Object, _busMock.Object);

            await reportingTaskHandler.Handle(message);

            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateReport>(), It.IsAny<CancellationToken>()), Times.Once());
            _autoMapperMock.Verify(x => x.Map<CreateReport>(It.IsAny<ReportingTaskMsg>()), Times.Once());
        }

        [Fact]
        public async Task ReportingTask_SendingMessageFailed_ThrowsException()
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReport>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();

            var message = new ReportingTaskMsg(Guid.NewGuid(), new List<ReportingTaskEntities> { ReportingTaskEntities.Task }, DateTime.Now, null);

            var reportingTaskHandler = new ReportingTaskHandler(_loggerMock.Object, _mediatorMock.Object, _autoMapperMock.Object, _busMock.Object);

            await Assert.ThrowsAsync<Exception>(() => reportingTaskHandler.Handle(message));

            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateReport>(), It.IsAny<CancellationToken>()), Times.Once());
            _autoMapperMock.Verify(x => x.Map<CreateReport>(It.IsAny<ReportingTaskMsg>()), Times.Once());
        }
    }
}
