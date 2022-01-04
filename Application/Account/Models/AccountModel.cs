using System.Collections.Generic;

namespace Application.Account.Models
{
    public class AccountModel
    {
        public int IDPERSON { get; set; }
        public string NAME { get; set; }
        public string CONTACT { get; set; }
        public string CNPJ { get; set; }
        public string TYPEPERSON { get; set; }
        public string TELONE { get; set; }
        public string TELTWO { get; set; }
        public string TELTHREE { get; set; }
        public string EMAIL { get; set; }
        public string DURESSPASSWORD { get; set; }
        public string HISTORICPERSIST { get; set; }
        public bool ACTIVE { get; set; }
        public string CEP { get; set; }
        public string ADDRESS { get; set; }
        public string NUMBER { get; set; }
        public string REFERENCE { get; set; }
        public string NEIGHBORHOOD { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string COUNTRY { get; set; }
        public List<AnnotationModel> ANNOTATIONS { get; set; }
        public List<ZoneModel> ZONES { get; set; }
        public List<EventModel> EVENTS { get; set; }
        public List<ScheduleModel> SCHEDULES { get; set; }
        public List<UnityModel> UNITYS { get; set; }
        public List<VehicleModel> VEHICLES { get; set; }
        public List<DeviceModel> DEVICES { get; set; }
        public List<CamDGuardModel> CAMSDGUARD { get; set; }
    }
}
