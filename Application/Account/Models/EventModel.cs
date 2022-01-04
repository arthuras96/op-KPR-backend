namespace Application.Account.Models
{
	public class EventModel
    {
		public int IDEVENT { get; set; }
		public string NAME { get; set; }
		public int IDPRIORITY { get; set; }
		public string PRIORITY { get; set; }
		public bool MANUALOCCURRENCE { get; set; }
		public bool MANUALCREATION { get; set; }
		public bool RECORDIMAGES { get; set; }
		public int IDZONE { get; set; }
		public bool ACTIVE { get; set; }
		public int IDPERSON { get; set; }
		public string INSTRUCTIONS { get; set; }
		//public string Actions { get; set; }
	}
}
