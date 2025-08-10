using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToDoApp_API.Interface;
using ToDoApp_API.Models;
using ToDoApp_API.Validation;

namespace ToDoApp_API.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _repo;

        public TaskService(ITaskRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<TaskModel> GetAll()
        {
            try
            {
                return _repo.GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                return new List<TaskModel>();
            }
        }

        public TaskModel GetById(Guid id)
        {
            try
            {
                return _repo.GetById(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                return null;
            }
        }

        public (bool Success, string Error, TaskModel Data) Add(TaskModel task)
        {
            try
            {
                var existingTasks = _repo.GetAll();
                var validation = new TaskValidation(existingTasks);
                var error = validation.Validate(task);
                if (error != null)
                    return (false, error, null);

                if (_repo.Exists(task.Name))
                {
                    return(false, "Task name already exists", null);
                }

                task.id = Guid.NewGuid();
                _repo.Add(task);
                return (true, null, task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Add: {ex}");
                return (false, "An unexpected error occurred while adding the task.", null);
            }

        }

        public (bool Success, string Error) Update(Guid id, TaskModel updated)
        {
            try
            {
                var task = _repo.GetById(id);
                {
                    if (task == null) return (false, "Task not found");
                }
                var existingTasks = _repo.GetAll();
                var validation = new TaskValidation(existingTasks);
                var error = validation.Validate(updated);
                if (error != null)
                    return (false, error);

                if (_repo.Exists(updated.Name, id))
                    return (false,"Task name already exists");

                updated.id = id;
                _repo.Update(updated);
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex}");
                return (false, "An unexpected error occurred while updating the task.");
            }
        }

        public (bool Success, string Error) Delete(Guid id)
        {
            try
            {
                var task = _repo.GetById(id);
                {
                    if (task == null) return (false, "Task not found");
                }

                if (task.Status != ToTaskStatus.Completed)
                {
                    return (false, "Only completed tasks can be deleted");
                }

                _repo.Delete(id);
                {
                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Delete: {ex}");
                return (false, "An unexpected error occurred while deleting the task.");
            }
        }
    }
}

