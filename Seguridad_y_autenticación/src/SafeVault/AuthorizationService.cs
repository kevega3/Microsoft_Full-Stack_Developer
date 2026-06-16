using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Authorization
{
    public class AuthorizationService
    {
        private readonly IDatabaseHelper _databaseHelper;

        private static readonly Dictionary<UserRole, HashSet<string>> RolePermissions = new()
        {
            { UserRole.User, new HashSet<string> { "read_profile", "update_profile" } },
            { UserRole.Admin, new HashSet<string> { "read_profile", "update_profile", "manage_users", "view_admin_panel", "delete_users", "update_roles" } }
        };

        public AuthorizationService(IDatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
        }

        public bool HasPermission(UserRole role, string permission)
        {
            if (string.IsNullOrEmpty(permission))
                return false;

            return RolePermissions.TryGetValue(role, out var permissions) && permissions.Contains(permission);
        }

        public bool HasPermission(User user, string permission)
        {
            if (user == null)
                return false;

            return HasPermission(user.Role, permission);
        }

        public bool CanAccessAdminPanel(User user)
        {
            if (user == null)
                return false;

            return HasPermission(user, "view_admin_panel");
        }

        public bool CanManageUsers(User user)
        {
            if (user == null)
                return false;

            return HasPermission(user, "manage_users");
        }

        public bool CanDeleteUsers(User user)
        {
            if (user == null)
                return false;

            return HasPermission(user, "delete_users");
        }

        public bool CanUpdateRoles(User user)
        {
            if (user == null)
                return false;

            return HasPermission(user, "update_roles");
        }

        public bool AssignRole(User adminUser, int targetUserId, UserRole newRole)
        {
            if (adminUser == null)
                return false;

            if (!CanUpdateRoles(adminUser))
                return false;

            var targetUser = _databaseHelper.GetAllUsers().FirstOrDefault(u => u.UserID == targetUserId);
            if (targetUser == null)
                return false;

            targetUser.Role = newRole;
            return _databaseHelper.UpdateUser(targetUser);
        }

        public List<string> GetUserPermissions(User user)
        {
            if (user == null)
                return new List<string>();

            if (RolePermissions.TryGetValue(user.Role, out var permissions))
                return permissions.ToList();

            return new List<string>();
        }

        public bool IsAuthenticated(User? user)
        {
            return user != null;
        }

        public bool Authorize(User? user, string requiredPermission)
        {
            if (!IsAuthenticated(user))
                return false;

            return HasPermission(user!, requiredPermission);
        }
    }
}