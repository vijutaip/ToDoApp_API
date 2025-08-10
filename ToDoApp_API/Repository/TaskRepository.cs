using ToDoApp_API.Interface;
using ToDoApp_API.Models;

namespace ToDoApp_API.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly List<TaskModel> _tasks = new();

        public IEnumerable<TaskModel> GetAll() => _tasks;

        public TaskModel GetById(Guid id) => _tasks.FirstOrDefault(t => t.id == id);

        public void Add(TaskModel task) => _tasks.Add(task);

        public void Update(TaskModel task)
        {
            var existing = GetById(task.id);
            if (existing != null)
            {
                existing.Name = task.Name;
                existing.Priority = task.Priority;
                existing.Status = task.Status;
            }
        }

        public void Delete(Guid id)
        {
            var task = GetById(id);
            if (task != null) _tasks.Remove(task);
        }

        public bool Exists(string name, Guid? ignoreId = null)
        {
            name = name.Trim().ToLower();
            return _tasks.Any(t => t.Name.Trim().ToLower() == name && (!ignoreId.HasValue || t.id != ignoreId.Value));
        }
    }

}
