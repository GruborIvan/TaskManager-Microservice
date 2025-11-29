using Moq;
using System;
using System.Threading;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.CommandHandlers
{
    public class RelateTaskToEntityHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository = new Mock<ITaskRepository>();
        private readonly Mock<IRelationRepository> _mockRelationRepository = new Mock<IRelationRepository>();
        private readonly Mock<RelateTaskToEntityValidator> _mockValidator = new Mock<RelateTaskToEntityValidator>();

        private static Guid _taskId = Guid.NewGuid();
        private Task TestTask { get; set; } = new Task(_taskId, default, default, default, default, default, default, default, default,
            new Assignment(null, "n", default), default);

        public RelateTaskToEntityHandlerTests()
        {
            _mockTaskRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestTask)
               .Verifiable();

            _mockTaskRepository.Setup(
                repository => repository.Update(It.IsAny<Task>()))
                .Returns(TestTask)
                .Verifiable();

            _mockTaskRepository.Setup(
                repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockValidator.Setup(
                validator => validator.ValidateAndThrow(It.IsAny<RelateTaskToEntity>()))
                .Verifiable();
        }

        [Fact]
        public async System.Threading.Tasks.Task Valid_Command_Adds_Relation()
        {
            //Arrange
            var command = new RelateTaskToEntity(Guid.NewGuid().ToString(), "type", _taskId, Guid.NewGuid());
            var relation = new Relation(Guid.NewGuid(), command.TaskId, command.EntityId, command.EntityType);

            var relateTaskToEntityHandler = new RelateTaskToEntityHandler(_mockTaskRepository.Object, _mockRelationRepository.Object, _mockValidator.Object);

            //Act
            _mockRelationRepository.Setup(
                repo => repo.AddAsync(It.IsAny<Relation>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(relation)
                .Verifiable();

            var newRelation = await relateTaskToEntityHandler.Handle(command, default);

            //Assert
            _mockTaskRepository.Verify(_ => _.GetAsync(It.Is<Guid>(t => t == command.TaskId), It.IsAny<CancellationToken>()), Times.Once);
            _mockRelationRepository.Verify(_ => _.AddAsync(It.IsAny<Relation>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRelationRepository.Verify(_ => _.SaveAsync(It.IsAny<CancellationToken>()));

            _mockTaskRepository.VerifyNoOtherCalls();
            _mockRelationRepository.VerifyNoOtherCalls();

            Assert.Equal(command.TaskId, newRelation.TaskId);
            Assert.Equal(command.EntityId, newRelation.EntityId);
            Assert.Equal(command.EntityType, newRelation.EntityType);
        }

        [Fact]
        public async System.Threading.Tasks.Task Invalid_TaskAlreadyFinalStatus()
        {
            //Arrange
            Task FinalTask = new Task(_taskId, default, default, default, default, default, default, default, default,
            new Assignment(null, "n", Guid.Empty), default, change: "Final", isFinal: true);

            _mockTaskRepository.Setup(
                repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(FinalTask);

            var command = new RelateTaskToEntity(Guid.NewGuid().ToString(), "type", _taskId, Guid.NewGuid());
            var relateTaskToEntityHandler = new RelateTaskToEntityHandler(_mockTaskRepository.Object, _mockRelationRepository.Object, _mockValidator.Object);

            //Act
            var exception = await Assert.ThrowsAsync<CannotModifyFinalizedTaskException>(async () => await relateTaskToEntityHandler.Handle(command, default));

            //Assert
            Assert.IsType<CannotModifyFinalizedTaskException>(exception);
        }
    }
}
