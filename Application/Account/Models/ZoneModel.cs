using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Account.Models
{
    public class ZoneModel
    {
        public int IDPERSON { get; set; }
        public int IDZONE { get; set; }
        public string ZONE { get; set; }
        public bool ACTIVE { get; set; }
        public bool ISRESTRICTED { get; set; }
    }
}
