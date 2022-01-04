using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Account.Models
{
    public class CamDGuardModel
    {
        public int IDCAMDGUARD { get; set; }
        public int IDACCOUNT { get; set; }
        public int IDDEVICEACCOUNT { get; set; }
        public string CAMNUMBER { get; set; }
        public string LAYOUT { get; set; }
        public string CAMNAME { get; set; }
        public int IDZONE { get; set; }
        public bool ACTIVEUSER { get; set; }
        public bool ACTIVERESIDENT { get; set; }
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
    }
}
