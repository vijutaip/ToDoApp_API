using ToDoApp_API.Models;

namespace ToDoApp_API.Interface
{
    
    public interface ITaskRepository
    {
        IEnumerable<TaskModel> GetAll();
        TaskModel GetById(Guid id);
        void Add(TaskModel task);
        void Update(TaskModel task);
        void Delete(Guid id);
        bool Exists(string name, Guid? ignoreId = null);
    }
}
