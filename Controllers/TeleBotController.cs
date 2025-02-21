using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotBaoKhach.Models;
using BotBaoKhach.Services;
using MongoDB.Bson;
using System.Security.Claims;
using BotBaoKhach.Common;


namespace BotBaoKhach.Controllers
{
    [ApiController]
    [Route("api/telebot")]
    [Authorize]
    public class TeleBotController : ControllerBase
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly ISettingBaoKhachService _service;

        public TeleBotController(
            ITelegramBotService telegramBotService, ISettingBaoKhachService service)
        {
            _telegramBotService = telegramBotService;
            _service = service;
        }




        [AllowAnonymous]
        [HttpPost("webhook/{botToken}")]
        public async Task<IActionResult> Post(string botToken,[FromBody] dynamic payload)
        {
            try
            {
                var response = await _telegramBotService.HandleWebHook(botToken, payload);
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


        [HttpGet("settings")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _service.GetAll(ObjectId.Parse(userIdClaim));
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

        [HttpPost("setting")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> Create([FromBody] SettingBaoKhachRequest model)
        {
            try
            {
                var response = await _service.CreateAsync(model);
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


        [HttpPut("setting/{id}")]
        [PermissionAuthorize(Permission.Edit)]
        public async Task<IActionResult> Update(string id, [FromBody] SettingBaoKhachRequest model)
        {
            try
            {
                var response = await _service.UpdateAsync(ObjectId.Parse(id), model);
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
        [HttpDelete("setting/{id}")]
        [PermissionAuthorize(Permission.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {

                var response = await _service.DeleteAsync(ObjectId.Parse(id));
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

        [HttpPost("setting/start/{id}")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> StartSetting(string id)
        {
            try
            {
                var response = await _service.Start(ObjectId.Parse(id));
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
        [HttpPost("setting/stop/{id}")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> StopSetting(string id)
        {
            try
            {
                var response = await _service.Stop(ObjectId.Parse(id));
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
