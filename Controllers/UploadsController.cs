using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotBaoKhach.Controllers
{
    [ApiController]
    [Route("api/upload-file")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadJsonFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (fileExtension != ".json")
                {
                    return BadRequest("Only JSON files are allowed.");
                }

                // Thư mục lưu file JSON
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/json");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName.Replace(" ", "")}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn file đã upload
                var fileUrl = $"/uploads/json/{uniqueFileName}";
                return Ok(new { Success = true, FileUrl = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
