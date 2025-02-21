using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BotBaoKhach.Common
{
    public class PermissionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly Permission _requiredPermission;

        public PermissionAuthorizeAttribute(Permission requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRoleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRoleClaim) || !Enum.TryParse(userRoleClaim, out UserRole userRole))
            {
                context.Result = new ObjectResult(new { message = "Bạn không có quyền truy cập" })
                {
                    StatusCode = (int)StatusCodeEnum.Forbidden
                };
                return;
            }
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var userPermissions = RolePermissions.GetPermissions(controllerName, userRole);

            if ((userPermissions & _requiredPermission) != _requiredPermission)
            {
                context.Result = new ObjectResult(new { message = "Bạn không có quyền thực hiện hành động này" })
                {
                    StatusCode = (int)StatusCodeEnum.Forbidden
                };
            }
        }
    }
}
