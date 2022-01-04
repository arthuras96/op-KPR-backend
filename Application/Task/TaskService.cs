using Application.General;
using Application.General.Entities;
using Application.General.Models;
using Application.Task.Models;
using Functions;
using Npgsql;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Application.Task
{
    public class TaskService
    {
        private Context context;
        private LogService ls;

        public GenericReturnModel AddTask(TaskModel Task, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            Task = queryUtils.SQLHandler(Task);
            ls = new LogService();
            string stQuery;

            bool MustInsert = CheckForTaskExistence(Task, IdPerson);

            if (!MustInsert)
            {
                stQuery = "INSERT INTO tbtaskopen (" +
                    "	 IDACCOUNT" +
                    "	,IDEVENT" +
                    "	,IDZONE" +
                    "	,IDPRIORITY" +
                    "	,DESCRIPTION" +
                    "	,ACUMULATE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + Task.IDACCOUNT +
                    "	," + Task.IDEVENT +
                    "	," + Task.IDZONE +
                    "	," + Task.IDPRIORITY +
                    "	," + queryUtils.InsertSingleQuotes(Task.DESCRIPTION) +
                    "	," + "1" +
                    "	);";


                context = new Context();

                try
                {
                    var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as IDTASK;");

                    if (retDB.HasRows)
                        statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "IDTASK"));

                    statusReturn.STATUSCODE = 201;
                    statusReturn.MESSAGE = "Tarefa criada com sucesso.";
                    ls.AddLogTask(int.Parse(IdPerson), Task.IDACCOUNT, "Inseriu a tarefa " + statusReturn.ID + " manualmente.");
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - AddTask (64)", e.Message, stQuery);
                    statusReturn.STATUSCODE = 500;
                    statusReturn.MESSAGE = "Falha ao salvar no banco de dados.";
                }

                context.Dispose();
            } else
            {
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Adição a tarefa realizada com sucesso!";
            }

            return statusReturn;
        }

        public AllTasksModel GetAllTasks(string IdPerson)
        {
            AllTasksModel Tasks = new AllTasksModel
            {
                OPENTASKS = GetTaskOpen(IdPerson),
                WORKTASKS = GetTaskWork(IdPerson),
                WAITTASKS = GetTaskWait(IdPerson)
            };

            return Tasks;
        }

        private List<TaskModel> GetTaskOpen(string IdPerson)
        {
            List<TaskModel> Tasks = new List<TaskModel>();
            ls = new LogService();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT " +
                    "	 tbtaskopen.IDTASK" +
                    "	,tbtaskopen.IDACCOUNT" +
                    "	,account.NAME as NAMEACCOUNT" +
                    "	,tbtaskopen.IDEVENT" +
                    "	,tbevent.NAME as TITLE" +
                    "	,tbevent.INSTRUCTIONS" +
                    "	,tbtaskopen.IDZONE" +
                    "	,tbtaskopen.IDPRIORITY" +
                    "	,tbtaskopen.DESCRIPTION" +
                    "	,tbtaskopen.DATETIMEOPEN" +
                    "	,tbtaskopen.ACUMULATE" +
                    " FROM tbtaskopen" +
                    " INNER JOIN tbperson as account ON account.IDPERSON = tbtaskopen.IDACCOUNT" +
                    " INNER JOIN tbevent             ON tbevent.IDEVENT = tbtaskopen.IDEVENT";

            context = new Context();

            try
            {
                var retDTOpen = context.RunCommandDT(stQuery);

                if (retDTOpen.Rows.Count > 0)
                {
                    Tasks = queryUtils.DataTableToList<TaskModel>(retDTOpen);
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - GetTaskOpen (132)", e.Message, stQuery);
            }

            context.Dispose();

            return Tasks;
        }

        private List<TaskModel> GetTaskWork(string IdPerson)
        {
            List<TaskModel> Tasks = new List<TaskModel>();
            ls = new LogService();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT " +
                    "	 tbtaskatwork.idtask" +
                    "	,tbtaskatwork.idaccount" +
                    "	,ACCOUNT.name as nameAccount" +
                    "	,tbtaskatwork.iduser" +
                    "	,USERSYS.name as user" +
                    "	,tbtaskatwork.idevent" +
                    "	,tbEVENT.name as title" +
                    "	,tbEVENT.instructions" +
                    "	,tbtaskatwork.idzone" +
                    "	,tbtaskatwork.idpriority" +
                    "	,tbtaskatwork.description" +
                    "	,tbtaskatwork.acumulate" +
                    "	,tbtaskatwork.datetimeopen" +
                    "	,tbtaskatwork.datetimeget" +
                    " FROM tbtaskatwork" +
                    " INNER JOIN tbPERSON as ACCOUNT ON ACCOUNT.idPerson = tbtaskatwork.idAccount" +
                    " INNER JOIN tbPERSON AS USERSYS ON USERSYS.idPerson = tbtaskatwork.idUser" +
                    " INNER JOIN tbEVENT             ON tbEVENT.idEvent = tbtaskatwork.idEvent";

            context = new Context();

            try
            {
                var retDTOpen = context.RunCommandDT(stQuery);

                if (retDTOpen.Rows.Count > 0)
                {
                    Tasks = queryUtils.DataTableToList<TaskModel>(retDTOpen);
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - GetTaskWork (196)", e.Message, stQuery);
            }

            context.Dispose();

            return Tasks;
        }

        private List<TaskModel> GetTaskWait(string IdPerson)
        {
            List<TaskModel> Tasks = new List<TaskModel>();
            ls = new LogService();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "SELECT " +
                    "	 tbtaskwait.idtask" +
                    "	,tbtaskwait.idaccount" +
                    "	,ACCOUNT.name as nameAccount" +
                    "	,tbtaskwait.iduser" +
                    "	,USERSYS.name as user" +
                    "	,tbtaskwait.idevent" +
                    "	,tbEVENT.name as title" +
                    "	,tbEVENT.instructions" +
                    "	,tbtaskwait.idzone" +
                    "	,tbtaskwait.idpriority" +
                    "	,tbtaskwait.description" +
                    "	,tbtaskwait.acumulate" +
                    "	,tbtaskwait.datetimeopen" +
                    "	,tbtaskwait.datetimeget" +
                    " FROM tbtaskwait" +
                    " INNER JOIN tbPERSON as ACCOUNT ON ACCOUNT.idPerson = tbtaskwait.idAccount" +
                    " INNER JOIN tbPERSON AS USERSYS ON USERSYS.idPerson	= tbtaskwait.iduser" +
                    " INNER JOIN tbEVENT			  ON tbEVENT.idEvent = tbtaskwait.idEvent";

            context = new Context();

            try
            {
                var retDTOpen = context.RunCommandDT(stQuery);

                if (retDTOpen.Rows.Count > 0)
                {
                    Tasks = queryUtils.DataTableToList<TaskModel>(retDTOpen);
                }

            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - GetTaskWait (260)", e.Message, stQuery);
            }

            context.Dispose();

            return Tasks;
        }

        public GenericReturnModel TaskOpenToWork(string IdTask, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "SELECT" +
                    "    IDTASK" +
                    "	,IDACCOUNT" +
                    "	,IDEVENT" +
                    "	,IDZONE" +
                    "	,IDPRIORITY" +
                    "	,DESCRIPTION" +
                    "	,DATETIMEOPEN" +
                    "	,ACUMULATE" +
                    " FROM tbtaskopen" +
                    " WHERE IDTASK = " + IdTask;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if(retDT.Rows.Count == 0)
                {                  
                    statusReturn.STATUSCODE = 204;
                    statusReturn.MESSAGE = "Está tarefa foi capturada por outro atendente.";
                    context.Dispose();
                    return statusReturn;
                }
                else
                {
                    task = queryUtils.DataTableToObject<TaskModel>(retDT);
                }
            }
            catch(Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskOpenToWork (189)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            stQuery = "DELETE FROM tbtaskopen WHERE IDTASK = " + IdTask;
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbtaskatwork (" +
                    "	 idtask" +
                    "	,idaccount" +
                    "	,iduser" +
                    "	,idevent" +
                    "	,idzone" +
                    "	,idpriority" +
                    "	,description" +
                    "	,datetimeopen" +
                    "	,acumulate" +
                    "	)" +
                    " VALUES (" +
                    "	 " + task.IDTASK +
                    "	," + task.IDACCOUNT +
                    "	," + IdPerson +
                    "	," + task.IDEVENT +
                    "	," + task.IDZONE +
                    "	," + task.IDPRIORITY +
                    "	," + queryUtils.InsertSingleQuotes(task.DESCRIPTION) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEOPEN)) +
                    "	," + task.ACUMULATE +
                    "	);";
            querys.Add(stQuery);

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "";
                    statusReturn.STATUSCODE = 201;
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskOpenToWork (366)", e.Message, stQuery);
            }

            return statusReturn;
        }
        
        public GenericReturnModel TaskWorkToWork(string IdTask, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "UPDATE tbtaskatwork set" +
                    "    idUser = " + IdPerson +
                    " WHERE idTask = " + IdTask;

            context = new Context();

            try
            {
                context.RunCommand(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Tarefa capturada com sucesso.";
            }
            catch (Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskWorkToWork (405)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            return statusReturn;
        }

        public GenericReturnModel TaskWorkToWait(string IdTask, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "SELECT" +
                    "	 idtask" +
                    "	,idaccount" +
                    "	,idevent" +
                    "	,idzone" +
                    "	,idpriority" +
                    "	,iduser" +
                    "	,description" +
                    "	,acumulate" +
                    "	,datetimeopen" +
                    "	,datetimeget" +
                    " FROM tbtaskatwork" +
                    " WHERE tbtaskatwork.idTask = " + IdTask +
                    " AND tbtaskatwork.idUser = " + IdPerson;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count == 0)
                {
                    statusReturn.STATUSCODE = 204;
                    statusReturn.MESSAGE = "Está tarefa foi capturada por outro atendente.";
                    context.Dispose();
                    return statusReturn;
                }
                else
                {
                    task = queryUtils.DataTableToObject<TaskModel>(retDT);
                }
            }
            catch (Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskWorkToWait (471)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            stQuery = "DELETE FROM tbtaskatwork WHERE idtask = " + IdTask;
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbtaskwait (" +
                    "	 idtask" +
                    "	,idaccount" +
                    "	,iduser" +
                    "	,idevent" +
                    "	,idzone" +
                    "	,idpriority" +
                    "	,description" +
                    "	,acumulate" +
                    "	,datetimeopen" +
                    "	,datetimeget" +
                    "	)" +
                    " VALUES (" +
                    "	 " + task.IDTASK +
                    "	," + task.IDACCOUNT +
                    "	," + IdPerson +
                    "	," + task.IDEVENT +
                    "	," + task.IDZONE +
                    "	," + task.IDPRIORITY +
                    "	," + queryUtils.InsertSingleQuotes(task.DESCRIPTION) +
                    "	," + task.ACUMULATE +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEOPEN)) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEGET)) +
                    "	);";

            querys.Add(stQuery);

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "";
                    statusReturn.STATUSCODE = 201;
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskOpenToWork (521)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public GenericReturnModel TaskWaitToWork(string IdTask, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "SELECT" +
                    "    idtask" +
                    "	,idaccount" +
                    "	,idevent" +
                    "	,idzone" +
                    "	,idpriority" +
                    "	,iduser" +
                    "	,description" +
                    "	,acumulate" +
                    "	,datetimeopen" +
                    "	,datetimeget" +
                    " FROM tbtaskwait" +
                    " WHERE idTask = " + IdTask;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count == 0)
                {
                    statusReturn.STATUSCODE = 204;
                    statusReturn.MESSAGE = "Está tarefa foi capturada por outro atendente.";
                    context.Dispose();
                    return statusReturn;
                }
                else
                {
                    task = queryUtils.DataTableToObject<TaskModel>(retDT);
                }
            }
            catch (Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskOpenToWork (579)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            stQuery = "DELETE FROM tbtaskwait WHERE idtask = " + IdTask;
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbtaskatwork (" +
                    "	 idtask" +
                    "	,idaccount" +
                    "	,iduser" +
                    "	,idevent" +
                    "	,idzone" +
                    "	,idpriority" +
                    "	,description" +
                    "	,acumulate" +
                    "	,datetimeopen" +
                    "	,datetimeget" +
                    "	)" +
                    " VALUES (" +
                    "	 " + task.IDTASK +
                    "	," + task.IDACCOUNT +
                    "	," + IdPerson +
                    "	," + task.IDEVENT +
                    "	," + task.IDZONE +
                    "	," + task.IDPRIORITY +
                    "	," + queryUtils.InsertSingleQuotes(task.DESCRIPTION) +
                    "	," + task.ACUMULATE +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEOPEN)) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEGET)) +
                    "	);";
            querys.Add(stQuery);

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "";
                    statusReturn.STATUSCODE = 201;
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskOpenToWork (632)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public GenericReturnModel TaskWaitToWait(string IdTask, string IdPerson)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "UPDATE tbtaskwait set" +
                    "    idUser = " + IdPerson +
                    " WHERE idTask = " + IdTask;

            context = new Context();

            try
            {
                context.RunCommand(stQuery);
                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Tarefa capturada com sucesso.";
            }
            catch (Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskWaitToWait (662)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            return statusReturn;
        }

        private bool CheckForTaskExistence (TaskModel Task, string IdPerson)
        {
            ls = new LogService();
            bool statusReturn = false;
            int idPriority = 0;
            int acumulate = 0;
            string description = "";
            string tableToUpdate = "";
            QueryUtils queryUtils = new QueryUtils();

            string stQuery;

            stQuery = "SELECT" +
                    "    idTask" +
                    "   ,idpriority" +
                    "   ,acumulate" +
                    "   ,description" +
                    " FROM tbtaskopen" +
                    " WHERE" +
                    "     idaccount = " + Task.IDACCOUNT +
                    " and idevent = " + Task.IDEVENT;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {

                    if(retDT.Rows[0]["idpriority"] == null) { } else { idPriority = Convert.ToInt32(retDT.Rows[0]["idpriority"]); }
                    if(retDT.Rows[0]["acumulate"] == null) { } else { acumulate = Convert.ToInt32(retDT.Rows[0]["acumulate"]); }
                    if(retDT.Rows[0]["description"] == null) { } else { description = retDT.Rows[0]["description"].ToString(); }
                    if(retDT.Rows[0]["idTask"] == null) { } else { Task.IDTASK = Convert.ToInt32(retDT.Rows[0]["idTask"]); }
                    tableToUpdate = "tbtaskopen";
                    statusReturn = true;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - CheckForTaskExistence (716)", e.Message, stQuery);
            }

            if (!statusReturn)
            {
                stQuery = "SELECT" +
                        "    idTask" +
                        "   ,idpriority" +
                        "   ,acumulate" +
                        "   ,description" +
                        " FROM tbtaskatwork" +
                        " WHERE" +
                        "     idaccount = " + Task.IDACCOUNT +
                        " and idevent = " + Task.IDEVENT;

                context = new Context();

                try
                {
                    var retDT = context.RunCommandDT(stQuery);
                    if (retDT.Rows.Count > 0)
                    {
                        if (retDT.Rows[0]["idpriority"] == null) { } else { idPriority = Convert.ToInt32(retDT.Rows[0]["idpriority"]); }
                        if (retDT.Rows[0]["acumulate"] == null) { } else { acumulate = Convert.ToInt32(retDT.Rows[0]["acumulate"]); }
                        if (retDT.Rows[0]["description"] == null) { } else { description = retDT.Rows[0]["description"].ToString(); ; }
                        if (retDT.Rows[0]["idTask"] == null) { } else { Task.IDTASK = Convert.ToInt32(retDT.Rows[0]["idTask"]); }
                        tableToUpdate = "tbtaskatwork";
                        statusReturn = true;
                    }
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - CheckForTaskExistence (744)", e.Message, stQuery);
                }
            }

            if (!statusReturn)
            {
                stQuery = "SELECT" +
                        "    idTask" +
                        "   ,idpriority" +
                        "   ,acumulate" +
                        "   ,description" +
                        " FROM tbtaskwait" +
                        " WHERE" +
                        "     idaccount = " + Task.IDACCOUNT +
                        " and idevent = " + Task.IDEVENT;

                context = new Context();

                try
                {
                    var retDT = context.RunCommandDT(stQuery);
                    if (retDT.Rows.Count > 0)
                    {
                        if (retDT.Rows[0]["idpriority"] == null) { } else { idPriority = Convert.ToInt32(retDT.Rows[0]["idpriority"]); }
                        if (retDT.Rows[0]["acumulate"] == null) { } else { acumulate = Convert.ToInt32(retDT.Rows[0]["acumulate"]); }
                        if (retDT.Rows[0]["description"] == null) { } else { description = retDT.Rows[0]["description"].ToString(); ; }
                        if (retDT.Rows[0]["idTask"] == null) { } else { Task.IDTASK = Convert.ToInt32(retDT.Rows[0]["idTask"]); }
                        tableToUpdate = "tbtaskwait";
                        statusReturn = true;
                    }
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - CheckForTaskExistence (773)", e.Message, stQuery);
                }
            }

            if (statusReturn)
            {
                acumulate++;

                if (idPriority > Task.IDPRIORITY)
                    Task.IDPRIORITY = idPriority;
                if (!string.IsNullOrEmpty(Task.DESCRIPTION))
                    description = Task.DESCRIPTION + "\n" + description;

                stQuery = "UPDATE " + tableToUpdate +
                        " SET " +
                        "    acumulate = " + acumulate +
                        "   ,idPriority = " + Task.IDPRIORITY +
                        "   ,description = " + queryUtils.InsertSingleQuotes(description) +
                        " WHERE idTask = " + Task.IDTASK;

                try
                {
                    context.RunCommand(stQuery);
                }
                catch (Exception e)
                {
                    ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - CheckForTaskExistence (803)", e.Message, stQuery);
                }
            }

            context.Dispose();

            return statusReturn;
        }

        public GenericReturnModel AddEditResolution(TaskResolutionModel taskResolution, string IdPerson, string IdAdmin)
        {
            QueryUtils queryUtils = new QueryUtils();
            taskResolution = queryUtils.SQLHandler(taskResolution);

            if (taskResolution.IDTASKRESOLUTION == 0)
                return AddResolution(taskResolution, IdPerson, IdAdmin);
            else
                return EditResolution(taskResolution, IdPerson, IdAdmin);
        }

        private GenericReturnModel AddResolution(TaskResolutionModel taskResolution, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            stQuery = "select SEQUENCE from tbtaskresolution where IDADMIN = " + IdAdmin + " order by SEQUENCE desc limit 1;";

            context = new Context();

            try { 
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                    taskResolution.SEQUENCE = Convert.ToInt32(retDT.Rows[0][0]) + 1;
                else
                {
                    taskResolution.SEQUENCE = 1;
                }
            }
            catch(Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - AddResolution (869)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao consultar banco de dados.";

                context.Dispose();

                return statusReturn;
            }

            stQuery = "INSERT INTO tbtaskresolution (" +
                    "	NAME" +
                    "	,TASKRESOLUTION" +
                    "	,SEQUENCE" +
                    "   ,IDADMIN" +
                    "	)" +
                    " VALUES (" +
                    "	 " + queryUtils.InsertSingleQuotes(taskResolution.NAME) +
                    "	," + queryUtils.InsertSingleQuotes(taskResolution.TASKRESOLUTION) +
                    "	," + taskResolution.SEQUENCE.ToString() + 
                    "   ," + IdAdmin +
                    "	);";

            try
            {
                var retDB = context.RunCommandRetID(stQuery + " SELECT LAST_INSERT_ID() as idtaskresolution;");

                if (retDB.HasRows)
                    statusReturn.ID = int.Parse(queryUtils.ReturnId(retDB, "idtaskresolution"));

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Tarefa criada com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), 0, "Inseriu a resolução " + statusReturn.ID + ".", IdLogSystemList.REGISTERRESOLUTION, "");
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - AddResolution (902)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao inserir no banco de dados.";
            }

            context.Dispose();
            return statusReturn;
        }

        private GenericReturnModel EditResolution(TaskResolutionModel taskResolution, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            string stQuery;

            stQuery = "UPDATE tbtaskresolution" +
                    "  SET" +
                    "	 NAME = " + queryUtils.InsertSingleQuotes(taskResolution.NAME) +
                    "	,TASKRESOLUTION = " + queryUtils.InsertSingleQuotes(taskResolution.TASKRESOLUTION) +
                    "  WHERE idtaskresolution = " + taskResolution.IDTASKRESOLUTION;

            string stringResolution = "";
            try
            {
                var resolutionBefore = GetResolutions(IdPerson, IdAdmin);
                int indexBefore = resolutionBefore.FindIndex(x => x.IDTASKRESOLUTION == taskResolution.IDTASKRESOLUTION);
                stringResolution = Newtonsoft.Json.JsonConvert.SerializeObject(resolutionBefore[indexBefore]);
            }
            catch (Exception e) { }

            context = new Context();

            try
            {
                context.RunCommand(stQuery);

                statusReturn.STATUSCODE = 201;
                statusReturn.MESSAGE = "Resolução alterada com sucesso.";
                ls.AddLogSystem(int.Parse(IdPerson), 0, "Alterou a resolução " + taskResolution.IDTASKRESOLUTION + ".", IdLogSystemList.EDITRESOLUTION, stringResolution);
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - EditResolution (936)", e.Message, stQuery);
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Falha ao consultar banco de dados.";
            }

            return statusReturn;
        }

        public GenericReturnModel UpSequenceResolution(TaskResolutionModel taskResolution, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            ls = new LogService();
            string stQuery;
            List<string> querys = new List<string>();

            // Altera a resolução de ordem imediata acima para -1
            stQuery = "UPDATE tbtaskresolution" +
                    "  SET" +
                    "	SEQUENCE = " + taskResolution.SEQUENCE.ToString() +
                    "  WHERE " +
                    "           SEQUENCE = " + (taskResolution.SEQUENCE - 1).ToString() + 
                    "       AND IDADMIN  = " + IdAdmin;

            querys.Add(stQuery);

            stQuery = "UPDATE tbtaskresolution" +
                    "  SET" +
                    "	SEQUENCE = " + (taskResolution.SEQUENCE - 1).ToString() +
                    "  WHERE " +
                    "           IDTASKRESOLUTION = " + taskResolution.IDTASKRESOLUTION +
                    "       AND IDADMIN          = " + IdAdmin;

            querys.Add(stQuery);

            context = new Context();

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "";
                    statusReturn.STATUSCODE = 201;
                    ls.AddLogSystem(int.Parse(IdPerson), 0, "Subiu na ordem a resolução " + taskResolution.IDTASKRESOLUTION + ". Ordem atual: " + (taskResolution.SEQUENCE - 1).ToString() + ".", IdLogSystemList.EDITRESOLUTION, "");
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - UpSequenceResolution (985)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public GenericReturnModel DownSequenceResolution(TaskResolutionModel taskResolution, string IdPerson, string IdAdmin)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            ls = new LogService();
            string stQuery;
            List<string> querys = new List<string>();

            // Altera a resolução de ordem imediata abaixo para +1
            stQuery = "UPDATE tbtaskresolution" +
                    "  SET" +
                    "	SEQUENCE = " + taskResolution.SEQUENCE.ToString() +
                    "  WHERE " +
                    "           SEQUENCE = " + (taskResolution.SEQUENCE + 1).ToString() +
                    "       AND IDADMIN  = " + IdAdmin;

            querys.Add(stQuery);

            stQuery = "UPDATE tbtaskresolution" +
                    "  SET" +
                    "	SEQUENCE = " + (taskResolution.SEQUENCE + 1).ToString() +
                    "  WHERE " +
                    "           IDTASKRESOLUTION = " + taskResolution.IDTASKRESOLUTION +
                    "       AND IDADMIN          = " + IdAdmin;

            querys.Add(stQuery);

            context = new Context();

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "";
                    statusReturn.STATUSCODE = 201;
                    ls.AddLogSystem(int.Parse(IdPerson), 0, "Abaixou na ordem a resolução " + taskResolution.IDTASKRESOLUTION + ". Ordem atual: " + (taskResolution.SEQUENCE - 1).ToString() + ".", IdLogSystemList.EDITRESOLUTION, "");
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - DownSequenceResolution (1033)", e.Message, stQuery);
            }

            return statusReturn;
        }

        public List<TaskResolutionModel> GetResolutions(string IdPerson, string IdAdmin)
        {
            List<TaskResolutionModel> ParamReturn = new List<TaskResolutionModel>();
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "select idTaskResolution, name, taskResolution, sequence from tbtaskresolution where IDADMIN = " + IdAdmin + " order by sequence;";

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count > 0)
                {
                    ParamReturn = queryUtils.DataTableToList<TaskResolutionModel>(retDT);
                }
            }
            catch (Exception e)
            {
                ls = new LogService();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Account Service - GetAccountList (692)", e.Message, stQuery);
            }
            context.Dispose();

            return ParamReturn;
        }

        public GenericReturnModel TaskWorkToClose(string IdTask, string IdPerson, string Resolution)
        {
            GenericReturnModel statusReturn = new GenericReturnModel();
            QueryUtils queryUtils = new QueryUtils();
            ls = new LogService();
            List<string> querys = new List<string>();
            string stQuery;
            TaskModel task = new TaskModel();

            stQuery = "SELECT" +
                    "	 IDTASK" +
                    "	,IDACCOUNT" +
                    "	,IDEVENT" +
                    "	,IDZONE" +
                    "	,IDPRIORITY" +
                    "	,IDUSER" +
                    "	,DESCRIPTION" +
                    "	,ACUMULATE" +
                    "	,DATETIMEOPEN" +
                    "	,DATETIMEGET" +
                    " FROM tbtaskatwork" +
                    " WHERE tbtaskatwork.IDTASK = " + IdTask +
                    " AND tbtaskatwork.IDUSER = " + IdPerson;

            context = new Context();

            try
            {
                var retDT = context.RunCommandDT(stQuery);
                if (retDT.Rows.Count == 0)
                {
                    statusReturn.STATUSCODE = 204;
                    statusReturn.MESSAGE = "Está tarefa foi alterada outro atendente.";
                    context.Dispose();
                    return statusReturn;
                }
                else
                {
                    task = queryUtils.DataTableToObject<TaskModel>(retDT);
                }
            }
            catch (Exception e)
            {
                statusReturn.STATUSCODE = 500;
                statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                context.Dispose();
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskWorkToWait (471)", e.Message, stQuery);
                return statusReturn;
            }

            context.Dispose();

            stQuery = "DELETE FROM tbtaskatwork WHERE IDTASK = " + IdTask;
            querys.Add(stQuery);

            stQuery = "INSERT INTO tbtaskclose (" +
                    "	 IDTASK" +
                    "	,IDACCOUNT" +
                    "	,IDUSER" +
                    "	,IDEVENT" +
                    "	,IDZONE" +
                    "	,IDPRIORITY" +
                    "	,DESCRIPTION" +
                    "	,ACUMULATE" +
                    "	,RESOLUTION" +
                    "	,DATETIMEOPEN" +
                    "	,DATETIMEGET" +
                    "	)" +
                    "VALUES (" +
                    "	 " + task.IDTASK +
                    "	," + task.IDACCOUNT +
                    "	," + IdPerson +
                    "	," + task.IDEVENT +
                    "	," + task.IDZONE +
                    "	," + task.IDPRIORITY +
                    "	," + queryUtils.InsertSingleQuotes(task.DESCRIPTION) +
                    "	," + task.ACUMULATE +
                    "   ," + queryUtils.InsertSingleQuotes(Resolution) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEOPEN)) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.DateBrToMySql(task.DATETIMEGET)) +
                    "	);";

            querys.Add(stQuery);

            try
            {
                ContextTransaction ct = new ContextTransaction();

                if (ct.RunTransaction(querys))
                {
                    statusReturn.MESSAGE = "Tarefa encerrada!";
                    statusReturn.STATUSCODE = 201;
                }
                else
                {
                    statusReturn.MESSAGE = "Houve uma falha ao processar a requisição. Por favor, tente novamente.";
                    statusReturn.STATUSCODE = 500;
                }
            }
            catch (Exception e)
            {
                ls.AddLogError(int.Parse(IdPerson), "Erro no Task Service - TaskWorkToClose (1084)", e.Message, stQuery);
            }

            return statusReturn;
        }
    }
}
