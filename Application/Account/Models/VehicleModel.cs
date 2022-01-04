using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Account.Models
{
    public class VehicleModel
    {
        public int IDVEHICLE { get; set; }
        public string LICENSEPLATE { get; set; }
        public string MODEL { get; set; }
        public string MANUFACTURER { get; set; }
        public string COLOR { get; set; }
        public string COMMENTS { get; set; }
        public int IDACCOUNT { get; set; }
        public bool ACTIVE { get; set; }
    }
}
