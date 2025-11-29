using GraphQL;
using GraphQL.Types;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.API.Controllers;
using Xunit;
using static TaskManager.API.Controllers.GraphQlController;

namespace TaskManager.Tests.UnitTests.Api
{
    public class GraphQlControllerTests
    {
        private GraphQlController _graphqlController { get; set; }

        private readonly Mock<IDocumentExecuter> _executerMock;
        private readonly Mock<ISchema> _schemaMock;

        public GraphQlControllerTests()
        {
            _executerMock = new Mock<IDocumentExecuter>();
            _executerMock.Setup(mediator =>
                    mediator.ExecuteAsync(It.IsAny<ExecutionOptions>()))
                .ReturnsAsync(new ExecutionResult
                {
                    Data = "result"
                });
            _schemaMock = new Mock<ISchema>();

            _graphqlController = new GraphQlController(_schemaMock.Object, _executerMock.Object);
        }

        [Fact]
        public async Task ReturnNotNullExecutionResult()
        {
            // Arrange
            var query = new PostBody { Query = @"{ ""query"": ""query { tasks { taskId } }""" };

            // Act
            var result = await _graphqlController.Post(query, default);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecutionResult>(result);
        }

        [Fact]
        public async Task Post_Returns_ExecutionResult()
        {
            // Arrange
            var request = new PostBody
            {
                OperationName = null,
                Query = null,
                Variables = null
            };

            // Act
            var result = await _graphqlController.Post(request, It.IsAny<CancellationToken>());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecutionResult>(result);
            Assert.Equal("result", result.Data.ToString());
            _executerMock.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionOptions>()), Times.Once());
        }

        [Fact]
        public async Task Get_Returns_ExecutionResult()
        {
            // Act
            var result = await _graphqlController.Get("", null, "", It.IsAny<CancellationToken>());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecutionResult>(result);
            Assert.Equal("result", result.Data.ToString());
            _executerMock.Verify(x => x.ExecuteAsync(It.IsAny<ExecutionOptions>()), Times.Once());
        }

        [Fact]
        public async Task Get_Returns_ExecutionException()
        {
            // Arrange
            _executerMock.Setup(mediator =>
                    mediator.ExecuteAsync(It.IsAny<ExecutionOptions>()))
                .ThrowsAsync(new Exception());

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _graphqlController.Get("", null, "", It.IsAny<CancellationToken>()));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
        }
    }
}
