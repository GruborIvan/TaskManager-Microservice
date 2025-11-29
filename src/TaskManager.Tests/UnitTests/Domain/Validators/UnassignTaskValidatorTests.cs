using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
   public class UnassignTaskValidatorTests
    {
        private readonly UnassignTaskValidator _validator = new UnassignTaskValidator();

        [Fact]
        public void Succeeds()
        {
            // Act
            var command = new UnassignTask(Guid.NewGuid(), Guid.NewGuid());
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Act
            UnassignTask command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(UnassignTask invalidCommand)
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
                yield return new UnassignTask[]
                {
                    new UnassignTask(Guid.Empty, Guid.NewGuid())
                };
            }
        }
    }
}
