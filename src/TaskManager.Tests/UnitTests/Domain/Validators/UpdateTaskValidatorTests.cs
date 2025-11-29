using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class UpdateTaskValidatorTests
    {
        private readonly UpdateTaskValidator _validator = new UpdateTaskValidator();

        [Fact]
        public void Valid_Command_Succeeds()
        {
            // Act
            var command = new UpdateTask(Guid.NewGuid(), "{\"name\":\"asd\"}", "status", Guid.NewGuid());
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Null_UpdateTask_Throws()
        {
            // Act
            UpdateTask command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Validate_Status_Missing_UpdateTask_Throws(UpdateTask invalidCommand)
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
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.Empty, string.Empty, string.Empty, default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.Empty, "{\"name\":\"asd\"}", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), string.Empty, "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "123", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "invalidData", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "{invalidData}", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "\"invalidData\"", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "'invalidData'", "status", default)
                };
                yield return new UpdateTask[]
                {
                    new UpdateTask(Guid.NewGuid(), "{\"name\":\"asd\"}", string.Empty, default)
                };
            }
        }
    }
}
