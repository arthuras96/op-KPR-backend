using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Resident.Models
{
    public class PersonAccessZoneModel
    {
        public int IDPERSON { get; set; }
        public int IDACCOUNT { get; set; }
        public int IDZONE { get; set; }
        public int IDSCHEDULE { get; set; }
        public bool ACCESS { get; set; }
        public string ZONE { get; set; }
    }
}
