using System;
using System.Linq;
using System.Collections.Generic;
using ToDoApp_API.Models;

namespace ToDoApp_API.Validation
{
    public class TaskValidation
    {
        private readonly IEnumerable<TaskModel> _existingTasks;

        public TaskValidation(IEnumerable<TaskModel> existingTasks)
        {
            _existingTasks = existingTasks;
        }

        public string Validate(TaskModel task)
        {
            if (task == null)
                return "Task cannot be null";

            if (string.IsNullOrWhiteSpace(task.Name))
                return "Task name is required";

            task.Name = task.Name.Trim(); 

            if (!System.Text.RegularExpressions.Regex.IsMatch(task.Name, @"^[a-zA-Z0-9\s_-]+$"))
                return "Task name cannot contain special characters";

            if (!Enum.IsDefined(typeof(ToTaskStatus), task.Status))
                return "Invalid task status";

            if (task.Priority < 1 || task.Priority > 100)
                return "Priority must be between 1 and 100";

            return null; 
        }
    }
}
