using FootyTipping.Server.Authorization;
using FootyTipping.Server.Models;
using FootyTipping.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootyTipping.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        // TODO: also readonly here?
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest request)
        {
            var response = _userService.Authenticate(request);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            _userService.Register(request);
            return Ok(new { message = "Registration successful." });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRequest request)
        {
            _userService.Update(id, request);
            return Ok(new { message = "User updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
