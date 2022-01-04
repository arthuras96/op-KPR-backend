using Application.Account.Models;
using System.Collections.Generic;

namespace Application.Resident.Models
{
    public class ResidentModel
    {
        // Person - Start Step 1
        public int IDPERSON { get; set; }
        public string NAME { get; set; }
        public int IDROLE { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public bool ACTIVE { get; set; }

        // Natural Person
        public string CPF { get; set; }
        public string RG { get; set; }
        public string BIRTHDATE { get; set; }
        public int IDGENDER { get; set; }

        // Resident
        public int IDACCOUNT { get; set; }
        public int IDTYPERESIDENT { get; set; }
        public bool ANSWERABLE { get; set; }
        public int SPONSOR { get; set; }
        public string COMPANY { get; set; }
        public string DEPARTMENT { get; set; }
        public string NOTE { get; set; }

        // Contact
        public string EMAIL { get; set; }
        public string TELONE { get; set; }
        public string TELTWO { get; set; }
        public string TELTHREE { get; set; }

        // N for N - Person from Unitys in Account - Step 2
        public List<UnityModel> RESIDENTUNITY { get; set; }

        // N for N - Person from Unitys in Account - Step 3
        public List<VehicleModel> RESIDENTVEHICLE { get; set; }

        // Access - Step 4
        public bool ACCESSPERMISSION { get; set; }
        public string ACCESSSTART { get; set; }
        public string ACCESSEND { get; set; }
        public List<PersonAccessZoneModel> ACCESSZONE { get; set; }
    }
}
