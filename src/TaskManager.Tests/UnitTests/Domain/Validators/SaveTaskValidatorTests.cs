using System;
using System.Collections.Generic;
using FluentValidation;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;
using Xunit;

namespace TaskManager.Tests.UnitTests.Domain.Validators
{
    public class SaveTaskValidatorTests
    {
        private static readonly Assignment _validAssignment = new Assignment(Guid.NewGuid(), "User", Guid.NewGuid());
        private readonly SaveTaskValidator _validator = new SaveTaskValidator();

        [Theory]
        [MemberData(nameof(ValidAssignments))]
        public void Validate_Valid_CreateTask_Succeeds(Assignment validAssignment)
        {
            // Act
            var command = new SaveTask("asdasd", Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", "New", validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default);
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [MemberData(nameof(ValidAssignments))]
        public void Validate_CallbackNull_CreateTask_Succeeds(Assignment validAssignment)
        {
            // Act
            var command = new SaveTask("asdasd", Guid.NewGuid(), "{}", null, "CreateTask", "New", validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default);
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.Null(exception);
        }

        public static IEnumerable<object[]> ValidAssignments
        {
            get
            {
                yield return new Assignment[]
                {
                    _validAssignment
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.Empty, "User", default)
                };
                yield return new Assignment[]
                {
                    new Assignment(null, "User", default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.NewGuid(), string.Empty, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.NewGuid(), null, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.Empty, null, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(Guid.Empty, string.Empty, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(null, null, default)
                };
                yield return new Assignment[]
                {
                    new Assignment(null, string.Empty, default)
                };
            }
        }

        [Fact]
        public void Validate_Null_CreateTask_Throws()
        {
            // Act
            SaveTask command = null;
            var exception = Record.Exception(() => _validator.ValidateAndThrow(command));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void Validate_CreateTask_With_Invalid_Commands_Throws(SaveTask invalidCommand)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(invalidCommand));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        [Theory]
        [MemberData(nameof(InvalidDataCommands))]
        public void Validate_CreateTask_With_Invalid_Data_Throws(SaveTask invalidDataCommand)
        {
            // Act
            var exception = Record.Exception(() => _validator.ValidateAndThrow(invalidDataCommand));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ValidationException>(exception);
        }

        public static IEnumerable<object[]> InvalidDataCommands
        {
            get
            {
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd", Guid.NewGuid(),"", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), " ", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "string", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{name: \"John Doe\"}", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), " {\"name: \"John Doe\"}", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), " {\"EntityRelations\":[{EntityId\":\"ae0daee2-d4c5-4695-ab2f-3adc4a621253\",\"EntityType\":\"Person\"}]}", "http://www.test.com", "CreateTask", "New", _validAssignment, default, default, new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",
                    Guid.NewGuid(),
                    "{\n\t\"personName\":\"Testqwe qwe\",\n\t\"title\":\"Review updated data for Testqwe qwe\",\n\t\"createdDate\":\"2020-12-15T13:15:39.4012223Z\",\n\t\"processName\":\"Update-Person\",\n\t\"description\":\"Please review person\"," +
                    "\n\t\"createdBy\":\"e787733b-8580-46da-80b6-f08112547fa1\",\n\t\"comments\":[],\n\t\"person\":{\n\t\"ProcessKey\":\"Update-Person\",\n\t\"CorrelationId\":\"d4049440-8e4e-4af5-93a5-f8194a32c266\",\n\t\"OperationId\":\"" +
                    "\"49053d47-5973-4acf-b261-5bc39e310a2b\",\n\t\"EntityRelations\":[{\"EntityId\":\"ae0daee2-d4c5-4695-ab2f-3adc4a621253\",\"EntityType\":\"Person\"}]\n}", 
                    "http://www.test.com", 
                    "CreateTask", "New",
                    _validAssignment, 
                    default, 
                    default, 
                    new Relation[] { new Relation(Guid.NewGuid(), "123", "Person") }, default, default, default)
                };
            }
        }

        public static IEnumerable<object[]> InvalidCommands
        {
            get
            {
                yield return new SaveTask[]
                {
                    new SaveTask(string.Empty,Guid.NewGuid(), string.Empty, string.Empty, string.Empty, string.Empty, _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask(string.Empty,Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), string.Empty, "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "123", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "invalidData", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "'invalidData'", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{asdfa}", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "\"invalidData\"", "http://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", string.Empty, "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", "invalidUrl", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", "ftp://www.test.com", "CreateTask", "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", "http://www.test.com", string.Empty, "New",  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", string.Empty,  _validAssignment, default, default, default, default, default, default)
                };
                yield return new SaveTask[]
                {
                    new SaveTask("asdasd",Guid.NewGuid(), "{}", "http://www.test.com", "CreateTask", "New",  null, default, default, default, default, default, default)
                };
            }
        }
    }
}
