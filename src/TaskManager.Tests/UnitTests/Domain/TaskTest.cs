using Moq;
using System;
using System.Linq;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Models;
using Xunit;
using Task = TaskManager.Domain.Models.Task;

namespace TaskManager.Tests.UnitTests.Domain
{
    public class TaskTest
    {
        private readonly Task _newTask;
        private readonly Mock<Callback> _mockCallback;
        public TaskTest()
        {
            _mockCallback = new Mock<Callback>();
            var taskType = "task-type";
            var fourEyeSubjectId = Guid.Empty;
            var subject = "";
            var source = new Source("", "");
            var comments = new Comment[0].AsEnumerable();
            var relations = new Relation[0].AsEnumerable();
            var status = "init";
            var data = "";
            var taskId = Guid.Empty;
            var assignment = new Assignment(Guid.Empty, "", default);

            _newTask = new Task(
                taskId,
                taskType,
                _mockCallback.Object,
                fourEyeSubjectId,
                subject,
                source,
                comments,
                status,
                data,
                assignment,
                relations);
        }

        [Fact]
        public void Task_Created_With_All_Paramters_Has_All_Parameters()
        {
            var taskType = "task-type";
            var callback = new HttpCallback(new Uri("https://uri.uri"));
            var fourEyeSubjectId = Guid.Empty;
            var subject = "";
            var source = new Source("", "");
            var comments = new Comment[0].AsEnumerable();
            var relations = new Relation[0].AsEnumerable();
            var status = "init";
            var data = "";
            var taskId = Guid.Empty;
            var assignment = new Assignment(Guid.Empty, "", default);

            var createdTask = new Task(
                taskId,
                taskType,
                callback,
                fourEyeSubjectId,
                subject,
                source,
                comments,
                status,
                data,
                assignment,
                relations);

            Assert.Equal(taskType, createdTask.TaskType);
            Assert.Equal(callback, createdTask.Callback);
            Assert.Equal(fourEyeSubjectId, createdTask.FourEyeSubjectId);
            Assert.Equal(subject, createdTask.Subject);
            Assert.Equal(source, createdTask.Source);
            Assert.Equal(comments, createdTask.Comments);
            Assert.Equal("Initial", createdTask.Change);
        }

        [Fact]
        public void Task_UpdateStatus_Updates_Status_And_Change_Is_Status()
        {
            var task = _newTask;
            var newStatus = "newStatus";

            task.UpdateStatus(newStatus, Guid.NewGuid());

            Assert.Equal(newStatus, task.Status);
            Assert.Equal("Status", task.Change);
        }


        [Fact]
        public void Task_FinalizeStatus_Updates_Status_And_Change_Is_Final()
        {
            var task = _newTask;
            var newStatus = "newStatus";

            task.FinalizeTask(newStatus, Guid.NewGuid());

            Assert.Equal(newStatus, task.Status);
            Assert.Equal("Final", task.Change);
            Assert.True(task.IsFinal);
        }

        [Fact]
        public void Task_UpdateStatus_Initiated_By_Four_Eye_Subject_Should_Not_Change_Status()
        {
            var task = _newTask;
            var newStatus = "newStatus";

            Assert.Throws<FourEyeRequirementNotMetException>(
                () => task.FinalizeTask(newStatus, initiatedBy: task.FourEyeSubjectId));

            Assert.NotEqual(newStatus, task.Status);
            Assert.NotEqual("Final", task.Change);
        }

        [Fact]
        public void Task_FinalizeTask_Sets_Final_Change()
        {
            var task = _newTask;
            var newStatus = "newStatus";

            task.FinalizeTask(newStatus, initiatedBy: Guid.NewGuid());
            Assert.Equal(newStatus, task.Status);
            Assert.Equal("Final", task.Change);
        }

        [Fact]
        public void Task_UpdateData_Updates_Data_And_Change_Is_Data()
        {
            var task = _newTask;
            var newData = "newStatus";

            task.UpdateData(newData, Guid.NewGuid());
            Assert.Equal(newData, task.Data);
            Assert.Equal("Data", task.Change);
        }

        [Fact]
        public void Task_UnAssign_Task_Nulls_AssignedToEntityId_And_Change_Is_Assignment()
        {
            var task = _newTask;
            task.Unassign(Guid.NewGuid());

            Assert.Null(task.Assignment.AssignedToEntityId);
            Assert.Equal("Assignment", task.Change);
        }

        [Fact]
        public void Task_UnAssing_Twice_Throws_TaskNotAssignedException()
        {
            var task = _newTask;
            task.Unassign(Guid.NewGuid());
            Assert.Throws<TaskNotAssignedException>(
                () => task.Unassign(Guid.NewGuid()));
        }
    }
}
