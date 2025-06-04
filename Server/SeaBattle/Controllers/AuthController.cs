using Microsoft.AspNetCore.Mvc;
using SeaBattle.Models;
using SeaBattle.Services;

namespace SeaBattle.Controllers
{
    /// <summary>
    /// Контроллер для управления аутентификацией и регистрацией пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера аутентификации
        /// </summary>
        /// <param name="userService">Сервис для работы с пользователями</param>
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Регистрирует нового пользователя в системе
        /// </summary>
        /// <param name="request">Данные для регистрации (имя пользователя и пароль)</param>
        /// <returns>Информация о созданном пользователе</returns>
        /// <response code="200">Пользователь успешно зарегистрирован</response>
        /// <response code="400">Пользователь с таким именем уже существует</response>
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

        /// <summary>
        /// Выполняет вход пользователя в систему
        /// </summary>
        /// <param name="request">Данные для входа (имя пользователя и пароль)</param>
        /// <returns>Информация о пользователе при успешном входе</returns>
        /// <response code="200">Вход выполнен успешно</response>
        /// <response code="401">Неверные учетные данные</response>
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

    /// <summary>
    /// Модель запроса для регистрации нового пользователя
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public required string Password { get; set; }
    }

    /// <summary>
    /// Модель запроса для входа в систему
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public required string Password { get; set; }
    }
} 