using Microsoft.AspNetCore.Mvc;
using ToDoApp_API.Models;
using ToDoApp_API.Services;


namespace ToDoApp_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _service;

        public TaskController(TaskService service)
        {
            _service = service;
        }

        // GET: api/task
        [HttpGet]
        public IActionResult GetAll()
        {
            var tasks = _service.GetAll();
            return Ok(tasks);
        }

        // GET: api/task/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var task = _service.GetById(id);
            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }

        // POST: api/task
        [HttpPost]
        public IActionResult Create(TaskModel task)
        {
            var (success, error, createdTask) = _service.Add(task);
            if (!success) return BadRequest(error);
            return Ok(createdTask);
        }

        // PUT: api/task/{id}
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, TaskModel task)
        {
            var (success, error) = _service.Update(id, task);
            if (!success) return BadRequest(error);
            {
                return Ok("Task updated successfully");
            }
        }

        // DELETE: api/task/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var (success, error) = _service.Delete(id);

            if (!success)
            {
                return BadRequest(error);
            }

            return Ok("Task deleted successfully");
        }
    }

}
