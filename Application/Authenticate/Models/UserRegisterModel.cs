using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Authenticate.Models
{
    public class UserRegisterModel
    {
        public int IDPERSON { get; set; }
        public string NAME { get; set; }
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public int IDUSERPROFILE { get; set; }
        public string USERNAME { get; set; }
        public bool ACTIVE { get; set; }
        public bool SENDACTIVEEMAIL { get; set; }
        public string USERPROFILE { get; set; }
    }
}
