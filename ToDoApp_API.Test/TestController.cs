using Xunit;
using ToDoApp_API.Controllers;
using ToDoApp_API.Models;
using Microsoft.AspNetCore.Mvc;
using ToDoApp_API.Services;
using ToDoApp_API.Repository;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using ToDoApp_API.Interface;

namespace ToDoApp_API.Test
{
    public class TaskControllerTests
    {
        private readonly TaskController _controller;
        private readonly Mock<ITaskRepository> _mockRepo;
        private readonly TaskService _service;

        public TaskControllerTests()
        {
            // 1. Create a mock repository
            _mockRepo = new Mock<ITaskRepository>();

            // 2. Pass the mock repo to service
            _service = new TaskService(_mockRepo.Object);

            // 3. Pass the service to controller
            _controller = new TaskController(_service);
        }

        //1.Testcase for check task list empty then return empty
        [Fact]
        public void GetTasks_WhenEmpty_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAll()).Returns(new List<TaskModel>());
            var actionResult = _controller.GetAll();
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult); // Ensure it’s not null
            var tasks = okResult.Value as List<TaskModel>;
            Assert.NotNull(tasks);
            Assert.Empty(tasks);
        }

        //2.get task by Id when add a multiple task in list

        [Fact]
        public void GetTasks_AfterAddingMultiple_ReturnsAll()
        {
            var tasks = new List<TaskModel>
    {
        new TaskModel { Name = "Task1" },
        new TaskModel { Name = "Task2" }
    };
            _mockRepo.Setup(r => r.GetAll()).Returns(tasks);

            var result = _service.GetAll();
            Assert.Equal(2, result.Count());
        }

        // 3. Test case for Get a valid task
        [Fact]
        public void GetTaskById_Valid_Success()
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel { id = id, Name = "Test" });

            var result = _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        //4. Get a invalid task
        [Fact]
        public void GetTaskById_Invalid_Fails()
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns((TaskModel)null);

            var result = _service.GetById(id);

            Assert.Null(result);
        }

        //5. Test case for add a valid task
        [Fact]
        public void AddTask_ValidTask_Returned()
        {
            var newTask = new TaskModel { Name = "Test Task", Priority = 1, Status = ToTaskStatus.NotStarted };
            _mockRepo.Setup(r => r.Add(It.IsAny<TaskModel>()))
                      .Callback((TaskModel task) => { });
            var actionResult = _controller.Create(newTask);
            var okResult = actionResult as OkObjectResult;
            Assert.NotNull(okResult);
            var addedTask = okResult.Value as TaskModel;
            Assert.NotNull(addedTask);
            Assert.Equal("Test Task", addedTask.Name);
            Assert.Equal(1, addedTask.Priority);
            Assert.Equal(ToTaskStatus.NotStarted, addedTask.Status);
        }

        //6. Test case for when enter duplicate task name enter
        [Fact]
        public void AddTask_DuplicateName_ReturnsBadRequest()
        {
            var existingTask = new TaskModel { Name = "Duplicate Task", Status = ToTaskStatus.NotStarted, Priority = 1 };
           
            _mockRepo.Setup(r => r.Exists("Duplicate Task", null)).Returns(true);

            var task = new TaskModel { Name = "Duplicate Task", Status = ToTaskStatus.NotStarted, Priority = 2 };
            var result = _controller.Create(task);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Task name already exists", badRequest.Value);
        }

        //7.Test case for Add a empty task name
        [Fact]
        public void AddTask_EmptyName_ReturnsBadRequest()
        {
            var task = new TaskModel { Name = "   ", Status = ToTaskStatus.NotStarted, Priority = 1 };
            var result = _controller.Create(task);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Task name is required", badRequest.Value);
        }

        //8.Test Case for Remove the whitespaces
        [Fact]
        public void AddTask_NameWithSpaces_TrimmedAndSuccess()
        {
            var task = new TaskModel { Name = "   My Task   ", Priority = 2 };
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), null)).Returns(false);

            var result = _service.Add(task);

            Assert.True(result.Success);
            Assert.Equal("My Task", task.Name);
        }

        //9. Test case for Update the data
        [Fact]
        public void UpdateTask_Valid_ReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel());
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), id)).Returns(false);
            var updatedTask = new TaskModel { Name = "New Name",Priority=1 };
            var result = _service.Update(id, updatedTask);
            Assert.True(result.Success);
            Assert.Null(result.Error);
        }

        //10. Test case for update duplicate task name
        [Fact]
        public void UpdateTask_DuplicateName_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel());
            _mockRepo.Setup(r => r.Exists("Duplicate Task", id)).Returns(true);
            var updatedTask = new TaskModel { Name = "Duplicate Task" };
            var result = _service.Update(id, updatedTask);
            Assert.False(result.Success);
            Assert.Equal("Task name already exists", result.Error);
        }
        //11.Test case forTask name not alllowed a null value
        [Fact]
        public void UpdateTask_NameIsNullOrEmpty_ReturnsError()
        {
           
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel());
            var updatedTask = new TaskModel { Name = "   " }; 
            var result = _service.Update(id, updatedTask);
            Assert.False(result.Success);
            Assert.Equal("Task name is required", result.Error);
            _mockRepo.Verify(r => r.Update(It.IsAny<TaskModel>()), Times.Never);
        }
        //12.Test case for update the Status
        [Fact]
        public void UpdateTask_StatusChange_Success()
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel { id = id, Status = ToTaskStatus.NotStarted });
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), id)).Returns(false);
            var result = _service.Update(id, new TaskModel { Name = "Test",Priority=1, Status = ToTaskStatus.Completed });
            Assert.True(result.Success);
        }

        //13.Test case for completed task deleted
        [Fact]
        public void DeleteTask_Completed_ReturnsSuccess()
        {
       
            var id = Guid.NewGuid();
            var task = new TaskModel { id = id, Status = ToTaskStatus.Completed };
            _mockRepo.Setup(r => r.GetById(id)).Returns(task);
            _mockRepo.Setup(r => r.Delete(id)); 
            var result = _service.Delete(id);
            Assert.True(result.Success);
            Assert.Null(result.Error);
            _mockRepo.Verify(r => r.Delete(id), Times.Once);
        }

        //14. Test case for Not Completed task not deleted
        [Fact]
        public void DeleteTask_NotCompleted_ReturnsError()
        {
            var id = Guid.NewGuid();
            var task = new TaskModel { id = id, Status = ToTaskStatus.InProgress };
            _mockRepo.Setup(r => r.GetById(id)).Returns(task);
            var result = _service.Delete(id);
            Assert.False(result.Success);
            Assert.Equal("Only completed tasks can be deleted", result.Error);
            _mockRepo.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        }

        //15.Test case for Task not found
        [Fact]
        public void DeleteTask_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns((TaskModel)null);
            var result = _service.Delete(id);
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Error);
            _mockRepo.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        }

        //16.Test case for Invalid null Id  for delete
        [Fact]
        public void DeleteTask_EmptyGuid_ReturnsError()
        {
            var emptyId = Guid.Empty;
            var result = _service.Delete(emptyId);
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Error);
            _mockRepo.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        }

        //17.Test case Set a default Nostarted Status if status is not selected
        [Fact]
        public void Status_DefaultsToNotStarted_IfNotProvided()
        {
            var task = new TaskModel { Name = "Default Status" };
            _mockRepo.Setup(r => r.Exists("Default Status", null)).Returns(false);

            _service.Add(task);

            Assert.Equal(ToTaskStatus.NotStarted, task.Status);
        }

        //18.Test Case for priority should be enter 1 to 100 range
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        public void Add_InvalidPriority_Fails(int priority)
        {
            var task = new TaskModel
            {
                Name = "Test Task",
                Priority = priority
            };
            var result = _service.Add(task);
            Assert.False(result.Success);
            Assert.Equal("Priority must be between 1 and 100", result.Error);
            _mockRepo.Verify(r => r.Add(It.IsAny<TaskModel>()), Times.Never);
        }

        //19.Test case for priority enter in between 1 t0 100 Add 
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Add_ValidPriority_Success(int priority)
        {
            var task = new TaskModel
            {
                Name = "Valid Task",
                Priority = priority
            };
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(),null)).Returns(false);
            var result = _service.Add(task);
            Assert.True(result.Success);
            Assert.Null(result.Error);
            Assert.Equal(priority, result.Data.Priority);
            _mockRepo.Verify(r => r.Add(It.IsAny<TaskModel>()), Times.Once);
        }

        // 20.Test case for priority should be enter 1 to 100 range Add

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Update_InvalidPriority_Fails(int priority)
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new TaskModel
            {
                id = taskId,
                Name = "Existing Task",
                Priority = 50
            };

            _mockRepo.Setup(r => r.GetById(taskId)).Returns(existingTask);

            var updatedTask = new TaskModel
            {
                Name = "Updated Task",
                Priority = priority
            };
            var result = _service.Update(taskId, updatedTask);
            Assert.False(result.Success);
            Assert.Equal("Priority must be between 1 and 100", result.Error);
            _mockRepo.Verify(r => r.Update(It.IsAny<TaskModel>()), Times.Never);
        }

        //21.Test case for priority enter in between 1 to 100
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Update_ValidPriority_Success(int priority)
        {
            var taskId = Guid.NewGuid();
            var existingTask = new TaskModel
            {
                id = taskId,
                Name = "Existing Task",
                Priority = 50
            };
            _mockRepo.Setup(r => r.GetById(taskId)).Returns(existingTask);
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), taskId)).Returns(false);
            var updatedTask = new TaskModel
            {
                Name = "Updated Task",
                Priority = priority
            };
            var result = _service.Update(taskId, updatedTask);
            Assert.True(result.Success);
            Assert.Null(result.Error);
            _mockRepo.Verify(r => r.Update(It.Is<TaskModel>(t => t.Priority == priority)), Times.Once);
        }

        //22.Test case for not allow special character when add
        [Theory]
        [InlineData("Task@123")]
        [InlineData("Hello!")]
        [InlineData("New#Task")]
        [InlineData("$$Money$$")]
        [InlineData("Clean%House")]
        public void Add_TaskNameWithSpecialCharacters_Fails(string invalidName)
        {
            var task = new TaskModel
            {
                Name = invalidName,
                Priority = 10
            };
            var result = _service.Add(task);
            Assert.False(result.Success);
            Assert.Equal("Task name cannot contain special characters", result.Error);
            _mockRepo.Verify(r => r.Add(It.IsAny<TaskModel>()), Times.Never);
        }

        //23.Test case for task name without special character
        [Theory]
        [InlineData("Valid Task 1")]
        [InlineData("Another123")]
        public void Add_TaskNameWithoutSpecialCharacters_Success(string validName)
        {
            var task = new TaskModel
            {
                Name = validName,
                Priority = 10
            };
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), null)).Returns(false);
            var result = _service.Add(task);
            Assert.True(result.Success);
            Assert.Null(result.Error);
            _mockRepo.Verify(r => r.Add(It.IsAny<TaskModel>()), Times.Once);
        }

        //24.Test case for task name not allow special character when update the task
        [Theory]
        [InlineData("Update@Task")]
        [InlineData("Hello!Update")]
        public void Update_TaskNameWithSpecialCharacters_Fails(string invalidName)
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel { id = id, Name = "Old Task", Priority = 20 });
            var updatedTask = new TaskModel
            {
                Name = invalidName,
                Priority = 20
            };
            var result = _service.Update(id, updatedTask);
            Assert.False(result.Success);
            Assert.Equal("Task name cannot contain special characters", result.Error);
            _mockRepo.Verify(r => r.Update(It.IsAny<TaskModel>()), Times.Never);
        }

        //25.Test case for task name without special character for update
        [Theory]
        [InlineData("ValidUpdate123")]
        [InlineData("Clean House")]
        public void Update_TaskNameWithoutSpecialCharacters_Success(string validName)
        {
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetById(id)).Returns(new TaskModel { id = id, Name = "Old Task", Priority = 20 });
            _mockRepo.Setup(r => r.Exists(It.IsAny<string>(), id)).Returns(false);

            var updatedTask = new TaskModel
            {
                Name = validName,
                Priority = 20
            };
            var result = _service.Update(id, updatedTask);
            Assert.True(result.Success);
            Assert.Null(result.Error);
            _mockRepo.Verify(r => r.Update(It.Is<TaskModel>(t => t.Name == validName)), Times.Once);
        }





    }



}