using System.ComponentModel;

namespace ToDoApp_API.Models
{
    public class TaskModel
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        [DefaultValue(1)]
        public int Priority { get; set; } = 1;
        public ToTaskStatus Status { get; set; }
    }
}
