using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class AssingmentValidatorTests
    {
        private readonly AssignmentValidator _validator = new AssignmentValidator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var validAssignment = new Assignment(Guid.NewGuid(), "User", default);

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(validAssignment));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Assignment_Throws()
        {
            // Act
            Assignment command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidAssignments))]
        public void Invalid_Assignment_Throws(Assignment invalidAssignment)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(invalidAssignment));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        public static IEnumerable<object[]> InvalidAssignments
        {
            get
            {
                yield return new Assignment[]
                {
                    new Assignment(Guid.NewGuid(), null, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.NewGuid(), string.Empty, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(null, "User", default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.Empty, "User", default)
                };
            }
        }
    }
}
