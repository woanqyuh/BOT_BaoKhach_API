using Microsoft.AspNetCore.Mvc;
using BotBaoKhach.Models;
using BotBaoKhach.Services;

namespace BotBaoKhach.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            try
            {

                var response = await _authService.LoginAsync(model);

                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }


        [HttpPost("refresh-token")]

        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest model)
        {

            try
            {

                var response = await _authService.RefreshTokenAsync(model);

                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }
    }
}