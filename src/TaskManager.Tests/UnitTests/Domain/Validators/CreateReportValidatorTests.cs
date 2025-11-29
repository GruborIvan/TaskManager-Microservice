using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class CreateReportValidatorTests
    {
        private readonly CreateReportValidator _validator = new CreateReportValidator();

        [Fact]
        public void Valid_CreateReportCommand_Succeeds()
        {
            // Act
            var command = new CreateReport(
                correlationId: Guid.NewGuid(),
                dboEntities: new List<string> { "Task", "Comment" },
                fromDatetime: DateTime.UtcNow.AddDays(-1),
                toDatetime: DateTime.UtcNow,
                initiatedBy: Guid.NewGuid());
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_Null_CreateReportCommand_ThrowsArgumentNullException()
        {
            // Act
            CreateReport command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Validate_Invalid_Commands_CreateReportCommand_ThrowsValidationException(CreateReport invalidCommand)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(invalidCommand));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        [Theory]
        [MemberData(nameof(ValidDates))]
        public void Validate_ValidDates_CreateReportCommand_Succeeds(CreateReport validDates)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow((CreateReport)validDates));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidDates))]
        public void Validate_InvalidDates_CreateReportCommand_ThrowsValidationException(CreateReport invalidDates)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow((CreateReport)invalidDates));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        public static IEnumerable<object[]> InvalidCommands
        {
            get
            {
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.Empty, new List<string> { "Task", "Comment" }, DateTime.MinValue, null, Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), null, DateTime.MinValue, null, Guid.NewGuid())
                };
            }
        }

        public static IEnumerable<object[]> ValidDates
        {
            get
            {
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, null, null, Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.AddDays(-1), null, Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1), Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), Guid.NewGuid())
                };
            }
        }

        public static IEnumerable<object[]> InvalidDates
        {
            get
            {
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, null, DateTime.Now.AddDays(-1), Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3), Guid.NewGuid())
                };
                yield return new CreateReport[]
                {
                    new CreateReport(Guid.NewGuid(), new List<string> { "Task", "Comment" }, DateTime.Now.Date, DateTime.Now.Date, Guid.NewGuid())
                };
            }
        }
    }
}
