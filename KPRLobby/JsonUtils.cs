using Application.Task.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KPRLobby
{
    public class JsonUtils
    {
        public void UpdateTasks(AllTasksModel tasks)
        {
            DateTime dateTimeNow = DateTime.Now;
            var sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
            tasks.TIMEREQUEST = sec;
            JObject auxObj = (JObject)JToken.FromObject(tasks);

            int control = 0;

            while (control < 15)
            {
                try
                {
                    File.WriteAllText(@".\Files\tasks.json", auxObj.ToString());
                    // write JSON directly to a file
                    using (StreamWriter file = File.CreateText(@".\Files\tasks.json"))
                    using (JsonTextWriter writer = new JsonTextWriter(file))
                    {
                        auxObj.WriteTo(writer);
                    }
                    control = 15;
                }
                catch
                {
                    control++;
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }
        }

        public AllTasksModel ReturnTasksStatic()
        {
            AllTasksModel tasks = new AllTasksModel();

            int control = 0;
            bool error = false;

            while (control < 5)
            {
                try
                {
                    tasks = JsonConvert.DeserializeObject<AllTasksModel>(File.ReadAllText(@".\Files\tasks.json"));
                    control = 5;
                    error = false;
                }
                catch
                {
                    control++;
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                    error = true;
                }
            }

            if (error)
                return null;
            
            return tasks;
        }
    }
}
