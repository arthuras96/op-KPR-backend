using Application.Authenticate.Entities;
using Application.General.Models;
using Application.Task;
using Application.Task.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPRLobby.Controllers
{
    [Route("api/task")]
    public class TaskController : Controller
    {
        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add")]
        public IActionResult AddTask([FromBody]TaskModel Task)
        {
            string idPerson = Request.Headers["idperson"];

            var addTask = new TaskService();
            return Ok(addTask.AddTask(Task, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get")]
        public IActionResult GetTask()
        {
            string idPerson = Request.Headers["idperson"];

            var getTask = new TaskService();
            var returnTasks = getTask.GetAllTasks(idPerson);
            
            JsonUtils jsonUpdater = new JsonUtils();
            jsonUpdater.UpdateTasks(returnTasks);

            return Ok(returnTasks);
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-static")]
        public IActionResult GetTaskStatic([FromQuery(Name = "Time")] long Time)
        {
            JsonUtils tasksReturn = new JsonUtils();
            AllTasksModel tasks = new AllTasksModel();

            string idPerson = Request.Headers["idperson"];

            if (Time == 0) {
                tasks = tasksReturn.ReturnTasksStatic();               
            }

            tasks = tasksReturn.ReturnTasksStatic();

            if(tasks == null)
            {
                var getTask = new TaskService();
                return Ok(getTask.GetAllTasks(idPerson));
            }

            if (tasks.TIMEREQUEST == Time)
                return Ok();

            return Ok(tasks);
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("open-to-work")]
        public IActionResult TaskOpenToWork([FromBody] GenericIdModel IdTask)
        {
            string idPerson = Request.Headers["idperson"];

            var openToWork = new TaskService();
            return Ok(openToWork.TaskOpenToWork(IdTask.ID.ToString(), idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("update-user-work")]
        public IActionResult TaskUpdateUserWork([FromBody] GenericIdModel IdTask)
        {
            string idPerson = Request.Headers["idperson"];

            var updateUser = new TaskService();
            return Ok(updateUser.TaskWorkToWork(IdTask.ID.ToString(), idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("work-to-wait")]
        public IActionResult TaskWorkToWait([FromBody] GenericIdModel IdTask)
        {
            string idPerson = Request.Headers["idperson"];

            var workToWait = new TaskService();
            return Ok(workToWait.TaskWorkToWait(IdTask.ID.ToString(), idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("wait-to-work")]
        public IActionResult TaskWaitToWork([FromBody] GenericIdModel IdTask)
        {
            string idPerson = Request.Headers["idperson"];

            var waitToWork = new TaskService();
            return Ok(waitToWork.TaskWaitToWork(IdTask.ID.ToString(), idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("update-user-wait")]
        public IActionResult TaskUpdateUserWait([FromBody] GenericIdModel IdTask)
        {
            string idPerson = Request.Headers["idperson"];

            var updateUser = new TaskService();
            return Ok(updateUser.TaskWaitToWait(IdTask.ID.ToString(), idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-resolution")]
        public IActionResult AddResolution([FromBody] TaskResolutionModel resolution)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var addResolution = new TaskService();
            return Ok(addResolution.AddEditResolution(resolution, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("up-resolution")]
        public IActionResult UpResolution([FromBody] TaskResolutionModel resolution)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var upResolution = new TaskService();
            return Ok(upResolution.UpSequenceResolution(resolution, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("down-resolution")]
        public IActionResult DownResolution([FromBody] TaskResolutionModel resolution)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var downResolution = new TaskService();
            return Ok(downResolution.DownSequenceResolution(resolution, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-resolution")]
        public IActionResult GetResolution()
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var getResolution = new TaskService();
            return Ok(getResolution.GetResolutions(idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("work-to-close")]
        public IActionResult TaskWorkToClose([FromBody] GenericParamModel resolution)
        {
            string idPerson = Request.Headers["idperson"];

            var workToClose = new TaskService();
            return Ok(workToClose.TaskWorkToClose(resolution.VALUE,idPerson, resolution.LABEL));
        }
    }
}