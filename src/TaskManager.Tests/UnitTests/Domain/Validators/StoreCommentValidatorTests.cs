using FluentValidation;
using System;
using System.Collections.Generic;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class StoreCommentValidatorTests
    {
        private readonly StoreCommentValidator _validator = new StoreCommentValidator();

        [Fact]
        public void Succeeds()
        {
            // Arrange 
            var command = new StoreComment(Guid.NewGuid(), "Test Comment", Guid.NewGuid(), DateTime.Now.AddSeconds(-2));

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Null_Command_Throws()
        {
            // Arrange 
            StoreComment command = null;

            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Invalid_Command_Throws(StoreComment invalidCommand)
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
                yield return new StoreComment[]
                {
                    new StoreComment(Guid.Empty, string.Empty, Guid.Empty, DateTime.MaxValue)
                };
                yield return new StoreComment[]
                {
                    new StoreComment(Guid.NewGuid(), string.Empty, Guid.NewGuid(), DateTime.UtcNow)
                };
                yield return new StoreComment[]
                {
                    new StoreComment(Guid.Empty, "test comment", Guid.NewGuid(), DateTime.UtcNow)
                };
                yield return new StoreComment[]
                {
                    new StoreComment(Guid.NewGuid(), "text", Guid.NewGuid(), DateTime.MaxValue)
                };
            }
        }
    }
}
