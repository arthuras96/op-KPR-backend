using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Authenticate.Models
{
    public class UserProfileModel
    {
        public int IDUSERPROFILE { get; set; }
        public string USERPROFILE { get; set; }
        public string DESCRIPTION { get; set; }
        public List<int> PERMISSION { get; set; }
    }
}
