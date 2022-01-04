using System.Collections.Generic;

namespace Application.Authenticate.Models
{
    public class User
    {
        public string NAME { get; set; }
        public int IDPERSON { get; set; }
        public string PASSWORD { get; set; }
        public string ROLE { get; set; }
        public string TOKEN { get; set; }
        public List<int> IDACCOUNT { get; set; }
        public int IDADMIN { get; set; }
        public List<int> PERMISSION { get; set; }
    }
}
