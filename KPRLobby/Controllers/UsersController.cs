using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Authenticate;
using Application.Authenticate.Models;
using Application.Authenticate.Entities;

namespace KPRLobby.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model, 0);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("authenticate-resident")]
        public IActionResult AuthenticateResident([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model, 1);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [Authorize(Roles = Role.ROOT)]
        [HttpPut("add-admin")]
        public IActionResult AddAccount([FromBody]UserRegisterModel user)
        {
            string idPerson = Request.Headers["idperson"];

            var addAdmin = new UserService();
            return Ok(addAdmin.AddEditAdmin(user, idPerson));
        }

        [Authorize(Roles = Role.ROOT)]
        [HttpGet("get-list-admin")]
        public IActionResult GetListAdmins()
        {
            string idPerson = Request.Headers["idperson"];

            var getAdmins = new UserService();
            return Ok(getAdmins.GetListAdmins(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-list-profiles")]
        public IActionResult GetListUserProfiles()
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var getProfiles = new UserService();
            return Ok(getProfiles.GetListProfile(idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-list-permissions")]
        public IActionResult GetListUserPermissions()
        {
            string idPerson = Request.Headers["idperson"];

            var getPermissions = new UserService();
            return Ok(getPermissions.GetListPermission(idPerson));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-user-profile")]
        public IActionResult AddUserProfile([FromBody]UserProfileModel profile)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var addUserProfile = new UserService();
            return Ok(addUserProfile.AddEditUserProfile(profile, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpPut("add-user")]
        public IActionResult AddUser([FromBody]UserRegisterModel user)
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var addUser = new UserService();
            return Ok(addUser.AddEditUser(user, idPerson, idAdmin));
        }

        [Authorize(Roles = Role.ADMIN + "," + Role.USER)]
        [HttpGet("get-list-user")]
        public IActionResult GetListUsers()
        {
            string idPerson = Request.Headers["idperson"];
            string idAdmin = Request.Headers["idadmin"];

            var getUsers = new UserService();
            return Ok(getUsers.GetListUsers(idPerson, idAdmin));
        }
    }
}