using Application.Account;
using Application.Account.Models;
using Application.Authenticate.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPRLobby.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add")]
        public IActionResult AddAccount([FromBody]AccountModel account)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idperson"];

            var addAccount = new AccountService();
            return Ok(addAccount.AddEditAccount(account, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-annotation")]
        public IActionResult AddAnotation([FromBody]AnnotationModel annotation)
        {
            string idPerson = Request.Headers["idperson"];

            var addAnnotation = new AccountService();
            return Ok(addAnnotation.AddAnnotation(annotation, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get")]
        public IActionResult GetAccounts()
        {
            string idPerson = Request.Headers["idperson"];

            var searchAccount = new AccountService();
            return Ok(searchAccount.GetAccounts(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-list")]
        public IActionResult GetAccountsList()
        {
            string idPerson = Request.Headers["idperson"];

            var searchAccount = new AccountService();
            return Ok(searchAccount.GetAccountList(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-complete")]
        public IActionResult GetCompleteAccount([FromQuery] string Id,
                                                [FromQuery] string Parameters
                                                )
        {
            string idPerson = Request.Headers["idperson"];

            var searchAccount = new AccountService();
            return Ok(searchAccount.GetCompleteAccount(idPerson, int.Parse(Id), Parameters));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-zone")]
        public IActionResult AddZone([FromBody]ZoneModel zone)
        {
            string idPerson = Request.Headers["idperson"];

            var addZone = new AccountService();
            return Ok(addZone.AddEditZone(zone, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-event")]
        public IActionResult AddEvent([FromBody]EventModel Event)
        {
            string idPerson = Request.Headers["idperson"];

            var addEvent = new AccountService();
            return Ok(addEvent.AddEditEvent(Event, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-schedule")]
        public IActionResult AddSchedule([FromBody]ScheduleModel Schedule)
        {
            string idPerson = Request.Headers["idperson"];

            var addSchedule = new AccountService();
            return Ok(addSchedule.AddEditSchedule(Schedule, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-type-unity")]
        public IActionResult GetTypeUnity()
        {
            string idPerson = Request.Headers["idperson"];

            var searchUnityParams = new AccountService();
            return Ok(searchUnityParams.GetTypeUnity(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-unity-state")]
        public IActionResult GetUnityState()
        {
            string idPerson = Request.Headers["idperson"];

            var searchUnityParams = new AccountService();
            return Ok(searchUnityParams.GetUnityState(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-unity")]
        public IActionResult AddUnity([FromBody]UnityModel Unity)
        {
            string idPerson = Request.Headers["idperson"];

            var addUnity = new AccountService();
            return Ok(addUnity.AddEditUnity(Unity, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-vehicle")]
        public IActionResult AddVehicle([FromBody]VehicleModel Vehicle)
        {
            string idPerson = Request.Headers["idperson"];

            var addVehicle = new AccountService();
            return Ok(addVehicle.AddEditVehicle(Vehicle, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-device-manufacturer")]
        public IActionResult GetDeviceManufacturer()
        {
            string idPerson = Request.Headers["idperson"];

            var getDeviceManufacturer = new AccountService();
            return Ok(getDeviceManufacturer.GetDeviceManufacturer(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-device-by-manufacturer")]
        public IActionResult GetDeviceByManufacturer([FromQuery] string Id)
        {
            string idPerson = Request.Headers["idperson"];

            var getDeviceByManufacturer = new AccountService();
            return Ok(getDeviceByManufacturer.GetDeviceByManufacturer(Id, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-device")]
        public IActionResult AddDevice([FromBody]DeviceModel Device)
        {
            string idPerson = Request.Headers["idperson"];

            if (Device == null)
                return BadRequest();

            var addDevice = new AccountService();
            return Ok(addDevice.AddEditDevice(Device, idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-camdguard")]
        public IActionResult AddCamDGuard([FromBody]CamDGuardModel CamDGuard)
        {
            string idPerson = Request.Headers["idperson"];

            if (CamDGuard == null)
                return BadRequest();

            var addCamDGuard = new AccountService();
            return Ok(addCamDGuard.AddEditCamDGuard(CamDGuard, idPerson));
        }

        [Authorize(Roles = Role.RESIDENT)]
        [HttpGet("get-accounts-for-resident")]
        public IActionResult GetAccountsFromId([FromQuery] string Id)
        {
            string idPerson = Request.Headers["idperson"];

            var getAccountsFromId = new AccountService();
            return Ok(getAccountsFromId.GetAccountsFromId(Id, idPerson));
        }

        [Authorize(Roles = Role.RESIDENT)]
        [HttpGet("get-cams-for-resident")]
        public IActionResult GetCamsForResident([FromQuery] string Id)
        {
            string idPerson = Request.Headers["idperson"];

            var getCamsForResident = new AccountService();
            return Ok(getCamsForResident.GetCamsForResident(Id, idPerson));
        }
    }
}