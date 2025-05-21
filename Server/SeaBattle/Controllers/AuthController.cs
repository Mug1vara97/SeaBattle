using Microsoft.AspNetCore.Mvc;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userService.GetUserByUsername(request.Username) != null)
            {
                return BadRequest("Пользователь с таким именем уже существует");
            }

            var user = await _userService.CreateUser(request.Username, request.Password);
            return Ok(new { user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!await _userService.ValidateUser(request.Username, request.Password))
            {
                return Unauthorized("Неверное имя пользователя или пароль");
            }

            var user = await _userService.GetUserByUsername(request.Username);
            return Ok(new { user!.Username });
        }
    }

    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
} 