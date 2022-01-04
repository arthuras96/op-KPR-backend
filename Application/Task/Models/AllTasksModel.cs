using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Task.Models
{
    public class AllTasksModel
    {
        public List<TaskModel> OPENTASKS { get; set; }
        public List<TaskModel> WORKTASKS { get; set; }
        public List<TaskModel> WAITTASKS { get; set; }
        public long TIMEREQUEST { get; set; }
    }
}
