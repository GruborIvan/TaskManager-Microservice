using FluentValidation;
using System;
using System.Collections.Generic;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class UpdateStatusValidatorTests
    {
        private readonly UpdateStatusValidator _validator = new UpdateStatusValidator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var command = new UpdateStatus(Guid.NewGuid(), "Done", Guid.NewGuid());

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Arrange 
            UpdateStatus command = null;

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(UpdateStatus invalidCommand)
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
                yield return new UpdateStatus[]
                {
                    new UpdateStatus(Guid.Empty, string.Empty, Guid.Empty)
                };
                yield return new UpdateStatus[]
                {
                    new UpdateStatus(Guid.NewGuid(), string.Empty, Guid.NewGuid())
                };
                yield return new UpdateStatus[]
                {
                    new UpdateStatus(Guid.Empty, "Done", Guid.NewGuid())
                };
            }
        }
    }
}

