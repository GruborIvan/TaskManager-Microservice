using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class UpdateTaskV2ValidatorTests
    {
        private readonly UpdateTaskV2Validator _validator = new UpdateTaskV2Validator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var command = new UpdateTaskV2(Guid.NewGuid(), "{\r\n  \"description\": \"Manual task description\"\r\n}", "Subject", Guid.NewGuid());

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Arrange 
            UpdateTaskV2 command = null;

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(UpdateTaskV2 invalidCommand)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(invalidCommand));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        public static IEnumerable<object[]> InvalidCommands
        {
            get
            {
                yield return new UpdateTaskV2[]
                {
                    new UpdateTaskV2(Guid.Empty, "{\r\n  \"description\": \"Manual task description\"\r\n}", "Subject", Guid.NewGuid())
                };
                yield return new UpdateTaskV2[]
                {
                    new UpdateTaskV2(Guid.NewGuid(), "test", "Subject", Guid.NewGuid())
                };
                yield return new UpdateTaskV2[]
                {
                    new UpdateTaskV2(Guid.NewGuid(), "", "Subject", Guid.NewGuid())
                };
            }
        }
    }
}
