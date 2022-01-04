using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Account.Models
{
    public class DeviceModel
    {
		public int IDDEVICEACCOUNT { get; set; }
		public int IDACCOUNT { get; set; }
		public string DEVICENAME { get; set; }
		public int IDDEVICEMANUFACTURER { get; set; }
		public int IDDEVICE { get; set; }
		public string DEVICE { get; set; }
		public string HOST { get; set; }
		public string PORT { get; set; }
		public string PORTSECONDARY { get; set; }
		public string USERNAME { get; set; }
		public string PASSWORD { get; set; }
		public string HOSTMONITORING { get; set; }
		public string PORTMONITORING { get; set; }
		public bool ACTIVE { get; set; }
		public string DEVICETYPE { get; set; }
	}
}
