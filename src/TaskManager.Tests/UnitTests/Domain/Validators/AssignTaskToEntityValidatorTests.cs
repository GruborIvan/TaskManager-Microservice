using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class AssignTaskToEntityValidatorTests
    {
        private readonly AssignTaskToEntityValidator _validator = new AssignTaskToEntityValidator();
        
        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var validAssignment = new Assignment(Guid.NewGuid(), "User", default);

            // Act
            var command = new AssignTaskToEntity(Guid.NewGuid(), validAssignment, Guid.NewGuid());
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Act
            AssignTaskToEntity command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(AssignTaskToEntity invalidCommand)
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
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.Empty, new Assignment(Guid.Empty, string.Empty, default), Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.Empty, new Assignment(Guid.NewGuid(), "User", default), Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.NewGuid(), null, Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.NewGuid(), new Assignment(Guid.NewGuid(), string.Empty, default), Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.NewGuid(), new Assignment(Guid.NewGuid(), null, default), Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.NewGuid(), new Assignment(null, "User", default), Guid.NewGuid())
                };
                yield return new AssignTaskToEntity[]
                {
                    new AssignTaskToEntity(Guid.NewGuid(), new Assignment(Guid.Empty, "User", default), Guid.NewGuid())
                };
            }
        }
    }
    
}
