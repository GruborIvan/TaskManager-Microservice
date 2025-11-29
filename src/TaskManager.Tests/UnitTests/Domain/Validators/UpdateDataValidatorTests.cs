using FluentValidation;
using System;
using System.Collections.Generic;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class UpdateDataValidatorTests
    {
        private readonly UpdateDataValidator _validator = new UpdateDataValidator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var command = new UpdateData(Guid.NewGuid(), "{}", Guid.NewGuid());

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Arrange 
            UpdateData command = null;

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(UpdateData invalidCommand)
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
                yield return new UpdateData[]
                {
                    new UpdateData(Guid.Empty, string.Empty, Guid.Empty)
                };
                yield return new UpdateData[]
                {
                    new UpdateData(Guid.NewGuid(), string.Empty, Guid.NewGuid())
                };
                yield return new UpdateData[]
                {
                    new UpdateData(Guid.NewGuid(), "not json", Guid.NewGuid())
                };
                yield return new UpdateData[]
                {
                    new UpdateData(Guid.Empty, "{}", Guid.NewGuid())
                };
                yield return new UpdateData[]
                {
                    new UpdateData(Guid.NewGuid(), "{name: \"John Doe\"}", Guid.NewGuid())
                };
            }
        }
    }
}
