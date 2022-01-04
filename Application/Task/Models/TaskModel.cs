using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Task.Models
{
    public class TaskModel
    {
		public int IDTASK { get; set; } //id
		public string NAMEACCOUNT { get; set; } //name
		public int IDACCOUNT { get; set; } //accountId
		public int IDEVENT { get; set; }
		public string TITLE { get; set; } //title
		public string USER { get; set; } //user
		public int IDUSER { get; set; } //user
		public int IDZONE { get; set; } //zone
		public int IDPRIORITY { get; set; } //priority
		public string DESCRIPTION { get; set; }
		public int ACUMULATE { get; set; }
		public string DATETIMEOPEN { get; set; } //hour
		public string DATETIMEGET { get; set; }
		public string DATETIMECLOSE { get; set; }
		public string INSTRUCTIONS { get; set; }
		
		//public Action ACTIONS { get; set; }
	}
}
