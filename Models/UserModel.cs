using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;


namespace BotBaoKhach.Models
{
    public class UserModel
    {

        public string Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public UserRole Role { get; set; }

        public List<PermissionModel> Permission { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserModel
    {
        [Required(ErrorMessage = "Fullname là bắt buộc.")]
        public string Fullname { get; set; }
        [Required(ErrorMessage = "TeleUser là bắt buộc.")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Permission là bắt buộc.")]
        public string[] Permission { get; set; }

    }

    public class AuthResponse
    {
        public UserModel User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class LoginResponse
    {
        public string ChatId { get; set; }
        public string UserId { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class VerifyCodeModel
    {
        [Required(ErrorMessage = "ChatId là bắt buộc.")]
        public string ChatId { get; set; }

        [Required(ErrorMessage = "Mã xác thực là bắt buộc.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "UserId là bắt buộc.")]
        public string UserId { get; set; }
    }

    public class ChangePasswordModel
    {

        [Required(ErrorMessage = "Password là bắt buộc.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password là bắt buộc.")]
        [MinLength(6, ErrorMessage = "New Password phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmNewPassword là bắt buộc.")]
        [Compare("NewPassword", ErrorMessage = "New Password và Confirm New Password phải khớp.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class PermissionModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PermissionRequest
    {
        public string Name { get; set; }
    }
}
