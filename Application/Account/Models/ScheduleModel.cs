namespace Application.Account.Models
{
    public class ScheduleModel
    {
        public int IDSCHEDULE { get; set; }
        public int IDPERSON { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public bool BOOLSUNDAY { get; set; }
        public bool BOOLMONDAY { get; set; }
        public bool BOOLTUESDAY { get; set; }
        public bool BOOLWEDNESDAY { get; set; }
        public bool BOOLTHURSDAY { get; set; }
        public bool BOOLFRIDAY { get; set; }
        public bool BOOLSATURDAY { get; set; }
        public TimeRangeModel TIMESUNDAY { get; set; }
        public TimeRangeModel TIMEMONDAY { get; set; }
        public TimeRangeModel TIMETUESDAY { get; set; }
        public TimeRangeModel TIMEWEDNESDAY { get; set; }
        public TimeRangeModel TIMETHURSDAY { get; set; }
        public TimeRangeModel TIMEFRIDAY { get; set; }
        public TimeRangeModel TIMESATURDAY { get; set; }

    }

}
