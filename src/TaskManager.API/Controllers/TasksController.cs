using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.API.Models;
using TaskManager.Domain.Commands;
using TaskManager.Domain.DomainEvents;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ITaskRepository _taskRepository;

        public TasksController(
            IMediator mediator,
            IMapper mapper,
            ILogger<TasksController> logger,
            ITaskRepository taskRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _taskRepository = taskRepository;
        }

        /// <summary>
        /// Returns the task with the specified ID
        /// </summary>
        /// <response code="200">If the task with the specified ID exists</response>
        /// <response code="400">If the ID has non-GUID format</response> 
        /// <response code="404">If there is no task with the specified ID</response> 
        [HttpGet("{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> GetTaskById(Guid taskId)
        {
            try
            {
                var result = await _taskRepository.GetAsync(taskId);
                bool hasHeader = Request.Headers.TryGetValue("x-user-id", out var userId);
                if (!hasHeader)
                    return BadRequest("No x-user-id header is present in the request.");
                var task = _mapper.Map<TaskDto>((result, Guid.Parse(userId)));

                return task;
            }
            catch(TaskNotFoundException)
            {
                _logger.LogError($"Task with Id: {taskId} not found.");
                return NotFound();
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Error while getting Task with Id: {taskId}.\nMessage: {e.Message}");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while getting Task with Id: {taskId}.\nMessage: {e.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Returns the historic list of tasks with the specified ID
        /// </summary>
        /// <response code="200">If the task with the specified ID exists</response>
        /// <response code="400">If the ID has non-GUID format or is empty GUID</response> 
        /// <response code="404">If there is no task with the specified ID</response> 
        [HttpGet("{taskId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto[]>> GetTaskHistoryByTaskId(Guid taskId)
        {
            try
            {
                var result = await _taskRepository.GetTaskHistoryAsync(taskId);
                Guid.TryParse(User?.Claims?.FirstOrDefault(w => w.Type == "UserId")?.Value, out var userId);
                var tasks = result.Select(task => _mapper.Map<TaskDto>((task, userId)));

                return tasks.ToArray();
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Error while getting Task with Id: {taskId}.\nMessage: {e.Message}");
                return BadRequest(e.Message);
            }
            catch (TaskNotFoundException e)
            {
                _logger.LogError($"Error while getting Task with Id: {taskId}.\nMessage: {e.Message}");
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while getting Task with Id: {taskId}.\nMessage: {e.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Registers a CreateTask message in the system
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Messages
        ///     {
        ///         "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "sourceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "sourceName": "Create-Person",
        ///         "subject": "string",
        ///         "data": "{\"name\": \"John Doe\"}",
        ///         "callback": "https://www.someurl.com",
        ///         "taskType": 0,
        ///         "status": "In Progress",
        ///         "assignedToEntityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "assignmentType": "string",
        ///         "relations": [
        ///             {
        ///                 "entityId": "123",
        ///                 "entityType": "Person"
        ///             }
        ///         ],
        ///         "fourEyeSubjectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "requestorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "taskId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "comment": "new comment"
        ///     }
        ///
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">If the CreateTask message was registered successfully</response>
        /// <response code="400">If there was an issue sending the CreateTask message</response> 
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SendCreateTaskMessage([FromBody] SendCreateTaskMessageRequest request)
        {
            try
            {
                var command = _mapper.Map<SaveTask>(request);
                var task = await _mediator.Send(command);
                Guid.TryParse(User?.Claims?.FirstOrDefault(w => w.Type == "UserId")?.Value, out var userId);
                return Ok(_mapper.Map<TaskDto>((task, userId)));
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Error while creating Task with CorrelationId: {request.CorrelationId}.\nMessage: {e.Message}");

                await _mediator.Publish(new CreateTaskFailed(request.TaskId ?? Guid.Empty, new ErrorData(e.Message, "")));

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while creating Task with CorrelationId: {request.CorrelationId}.\nMessage: {e.Message}");

                await _mediator.Publish(new CreateTaskFailed(request.TaskId ?? Guid.Empty, new ErrorData(e.Message, "")));

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}