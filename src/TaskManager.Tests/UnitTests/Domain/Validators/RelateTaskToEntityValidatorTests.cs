using FluentValidation;
using System;
using System.Collections.Generic;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class RelateTaskToEntityValidatorTests
    {
        private readonly RelateTaskToEntityValidator _validator = new RelateTaskToEntityValidator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var command = new RelateTaskToEntity(Guid.NewGuid().ToString(), "Person", Guid.NewGuid(), Guid.NewGuid());

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Arrange 
            RelateTaskToEntity command = null;

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(RelateTaskToEntity invalidCommand)
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
                yield return new RelateTaskToEntity[]
                {
                    new RelateTaskToEntity(Guid.Empty.ToString(), string.Empty, Guid.Empty, Guid.Empty)
                };
                yield return new RelateTaskToEntity[]
                {
                    new RelateTaskToEntity(Guid.NewGuid().ToString(), "Person", Guid.Empty, Guid.NewGuid())
                };
                yield return new RelateTaskToEntity[]
                {
                    new RelateTaskToEntity(Guid.NewGuid().ToString(), string.Empty, Guid.NewGuid(), Guid.NewGuid())
                };
                yield return new RelateTaskToEntity[]
                {
                    new RelateTaskToEntity(string.Empty, "Person", Guid.NewGuid(), Guid.NewGuid())
                };
            }
        }
    }
}
