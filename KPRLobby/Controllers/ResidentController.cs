using Application.Authenticate.Entities;
using Application.Resident;
using Application.Resident.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPRLobby.Controllers
{
    [Route("api/resident")]
    public class ResidentController : Controller
    {
        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("preload")]
        public IActionResult Preload([FromQuery] string Id)
        {
            string idPerson = Request.Headers["idperson"];

            var loadParams = new ResidentService();
            return Ok(loadParams.GetPreload(Id, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add")]
        public IActionResult AddResident([FromBody]ResidentModel Resident)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            if (Resident == null)
                return BadRequest();

            var addResident = new ResidentService();
            return Ok(addResident.AddEditResident(Resident, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-list")]
        public IActionResult GetListResidents([FromQuery] string Id)
        {
            string idPerson = Request.Headers["idperson"];

            var loadResidents = new ResidentService();
            return Ok(loadResidents.GetListResidents(Id, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("get-complete")]
        public IActionResult GetCompleteResident([FromBody] ResidentModel Resident)
        {
            string idPerson = Request.Headers["idperson"];

            if (Resident.IDPERSON == 0 && string.IsNullOrEmpty(Resident.CPF))
                return BadRequest();

            var GetResident = new ResidentService();
            return Ok(GetResident.GetCompleteResident(Resident, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPost("get-complete-from-cpf")]
        public IActionResult GetCompleteResidentFromCPF([FromBody] ResidentModel Resident)
        {
            string idPerson = Request.Headers["idperson"];

            if (string.IsNullOrEmpty(Resident.CPF))
                return BadRequest();

            var GetResident = new ResidentService();
            return Ok(GetResident.GetCompleteResidentFromCPF(Resident, idPerson));
        }
    }
}