using Functions;
using Npgsql;
using Repository;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Application.General
{
    public class LogService
    {
        private Context context;

        public bool AddLogSystem(int executant, int affected, string action, int idlogsystemtype, string jsonpreviousstate)
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();

            stQuery = "INSERT INTO tblogsystem (" +
                    "	 EXECUTANT" +
                    "	,AFFECTED" +
                    "	,ACTION" +
                    "   ,IDLOGSYSTEMTYPE" +
                    "   ,JSONPREVIOUSSTATE" +
                    "	)" +
                    " VALUES (" +
                    "	 " + executant.ToString() +
                    "	," + affected.ToString() +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.SecureStr(action)) +
                    "   ," + idlogsystemtype.ToString() +
                    "   ," + queryUtils.InsertSingleQuotes(jsonpreviousstate) +
                    "	);";

            context = new Context();
            try
            {
                context.RunCommand(stQuery);
                context.Dispose();
                return true;
            }
            catch (Exception e)
            {
                AddLogError(executant, "LogService - AddLogSystem", e.Message, stQuery);
                context.Dispose();
                return false;
            }
        }

        public bool AddLogError(int executant, string action, string message, string executeQuery, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = "")
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();
            executeQuery = executeQuery.Replace("'", "''");

            stQuery = "INSERT INTO tblogerror (" +
                    "	 EXECUTANT" +
                    "	,ACTION" +
                    "	,MESSAGE" +
                    "   ,SQLQUERY" +
                    "	)" +
                    " VALUES (" +
                    "	 " + executant.ToString() +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.SecureStr(action.ToString() + " - Line: " + lineNumber.ToString() + " | Caller: " + caller)) +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.SecureStr(message)) +
                    "   ," + queryUtils.InsertSingleQuotes(executeQuery) +
                    "	);";

            context = new Context();
            try
            {
                context.RunCommand(stQuery);
                context.Dispose();
                return true;
            }
            catch
            {
                context.Dispose();
                return false;
            }
        }

        public bool AddLogTask(int executant, int affected, string action)
        {
            string stQuery;
            QueryUtils queryUtils = new QueryUtils();


            stQuery = "INSERT INTO tblogtask (" +
                    "	 executant" +
                    "	,affected" +
                    "	,action" +
                    "	)" +
                    " VALUES (" +
                    "	 " + executant.ToString() +
                    "	," + affected.ToString() +
                    "	," + queryUtils.InsertSingleQuotes(queryUtils.SecureStr(action)) +
                    "	);";

            if (affected == 0)
                stQuery = "INSERT INTO tblogtask (" +
                        "	 executant" +
                        "	,action" +
                        "	)" +
                        " VALUES (" +
                        "	 " + executant.ToString() +
                        "	," + queryUtils.InsertSingleQuotes(queryUtils.SecureStr(action)) +
                        "	);";


            context = new Context();
            try
            {
                context.RunCommand(stQuery);
                context.Dispose();
                return true;
            }
            catch (Exception e)
            {
                AddLogError(executant, "LogService - AddLogTask", e.Message, stQuery);
                context.Dispose();
                return false;
            }
        }
    }
}
