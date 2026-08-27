using Microsoft.AspNetCore.Mvc;
using RequestLifeCycle.Dtos.UserDto;
using RequestLifeCycle.services;
using RequestLifeCycle.services.user;

namespace RequestLifeCycle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUser _userService;

        public UsersController(IUser userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _userService.register(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _userService.login(request);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _userService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    }
}