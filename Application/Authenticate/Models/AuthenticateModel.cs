using System.ComponentModel.DataAnnotations;

namespace Application.Authenticate.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string USERNAME { get; set; }

        [Required]
        public string PASSWORD { get; set; }
    }
}
