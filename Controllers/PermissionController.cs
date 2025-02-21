using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotBaoKhach.Models;
using BotBaoKhach.Services;

using System.Security.Claims;
using MongoDB.Bson;
using Telegram.Bot.Types;
using BotBaoKhach.Common;

namespace BotBaoKhach.Controllers
{
    [ApiController]
    [Route("api/permission")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _service;


        public PermissionController(IPermissionService service)
        {
            _service = service;

        }

        [HttpGet]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _service.GetAll();
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

        [HttpPost]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> Create([FromBody] PermissionRequest model)
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


        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.Edit)]
        public async Task<IActionResult> Update(string id, [FromBody] PermissionRequest model)
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
        [HttpDelete("{id}")]
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

    }
}
