using System.Security;

namespace BotBaoKhach.Common
{
    public static class RolePermissions
    {
        private static readonly Permission allPermissions = Permission.View | Permission.Edit | Permission.Delete | Permission.Create | Permission.ViewMe;

        public static readonly Dictionary<string, Dictionary<UserRole, Permission>> PermissionsByController = new()
        {
            ["User"] = new Dictionary<UserRole, Permission>
            {  
                { UserRole.Manager, Permission.None | Permission.ViewMe },     
                { UserRole.Admin, allPermissions },     
            },
            ["Permission"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Manager, Permission.None },
                { UserRole.Admin, allPermissions },
            },
            ["TeleBot"] = new Dictionary<UserRole, Permission>
            {
                { UserRole.Manager, Permission.View | Permission.Edit | Permission.Delete | Permission.Create },
                { UserRole.Admin, allPermissions }
            },
        };

        public static Permission GetPermissions(string controllerName, UserRole role)
        {
            if (PermissionsByController.TryGetValue(controllerName, out var rolePermissions))
            {
                return rolePermissions.TryGetValue(role, out var permission) ? permission : Permission.None;
            }

            return Permission.None;
        }
    }
}
