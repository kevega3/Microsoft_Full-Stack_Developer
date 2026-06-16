using Xunit;
using SafeVault.Authorization;
using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Tests
{
    public class TestAuthorization
    {
        private readonly AuthorizationService _authorizationService;
        private readonly InMemoryDatabaseHelper _databaseHelper;

        public TestAuthorization()
        {
            _databaseHelper = new InMemoryDatabaseHelper();
            _databaseHelper.InitializeDatabase();
            _authorizationService = new AuthorizationService(_databaseHelper);
        }

        [Fact]
        public void TestHasPermission_AdminHasAllPermissions()
        {
            var adminPermissions = _authorizationService.GetUserPermissions(new User { Role = UserRole.Admin });
            Assert.Contains("manage_users", adminPermissions);
            Assert.Contains("view_admin_panel", adminPermissions);
            Assert.Contains("delete_users", adminPermissions);
            Assert.Contains("update_roles", adminPermissions);
            Assert.Contains("read_profile", adminPermissions);
            Assert.Contains("update_profile", adminPermissions);
        }

        [Fact]
        public void TestHasPermission_UserHasLimitedPermissions()
        {
            var userPermissions = _authorizationService.GetUserPermissions(new User { Role = UserRole.User });
            Assert.Contains("read_profile", userPermissions);
            Assert.Contains("update_profile", userPermissions);
            Assert.DoesNotContain("manage_users", userPermissions);
            Assert.DoesNotContain("view_admin_panel", userPermissions);
            Assert.DoesNotContain("delete_users", userPermissions);
            Assert.DoesNotContain("update_roles", userPermissions);
        }

        [Fact]
        public void TestCanAccessAdminPanel_AdminCanAccess()
        {
            var admin = new User { Role = UserRole.Admin };
            Assert.True(_authorizationService.CanAccessAdminPanel(admin));
        }

        [Fact]
        public void TestCanAccessAdminPanel_UserCannotAccess()
        {
            var user = new User { Role = UserRole.User };
            Assert.False(_authorizationService.CanAccessAdminPanel(user));
        }

        [Fact]
        public void TestCanManageUsers_AdminCanManage()
        {
            var admin = new User { Role = UserRole.Admin };
            Assert.True(_authorizationService.CanManageUsers(admin));
        }

        [Fact]
        public void TestCanManageUsers_UserCannotManage()
        {
            var user = new User { Role = UserRole.User };
            Assert.False(_authorizationService.CanManageUsers(user));
        }

        [Fact]
        public void TestCanDeleteUsers_AdminCanDelete()
        {
            var admin = new User { Role = UserRole.Admin };
            Assert.True(_authorizationService.CanDeleteUsers(admin));
        }

        [Fact]
        public void TestCanDeleteUsers_UserCannotDelete()
        {
            var user = new User { Role = UserRole.User };
            Assert.False(_authorizationService.CanDeleteUsers(user));
        }

        [Fact]
        public void TestIsAuthenticated_AuthenticatedUser()
        {
            var user = new User { Username = "testuser" };
            Assert.True(_authorizationService.IsAuthenticated(user));
        }

        [Fact]
        public void TestIsAuthenticated_NullUser()
        {
            Assert.False(_authorizationService.IsAuthenticated(null));
        }

        [Fact]
        public void TestAuthorize_AdminCanAccessAdminResources()
        {
            var admin = new User { Role = UserRole.Admin };
            Assert.True(_authorizationService.Authorize(admin, "view_admin_panel"));
            Assert.True(_authorizationService.Authorize(admin, "manage_users"));
        }

        [Fact]
        public void TestAuthorize_UserCannotAccessAdminResources()
        {
            var user = new User { Role = UserRole.User };
            Assert.False(_authorizationService.Authorize(user, "view_admin_panel"));
            Assert.False(_authorizationService.Authorize(user, "manage_users"));
        }

        [Fact]
        public void TestAuthorize_UnauthenticatedUser()
        {
            Assert.False(_authorizationService.Authorize(null, "read_profile"));
        }

        [Fact]
        public void TestGetUserPermissions_AdminRole()
        {
            var admin = new User { Role = UserRole.Admin };
            var permissions = _authorizationService.GetUserPermissions(admin);

            Assert.Equal(6, permissions.Count);
            Assert.Contains("read_profile", permissions);
            Assert.Contains("update_profile", permissions);
            Assert.Contains("manage_users", permissions);
            Assert.Contains("view_admin_panel", permissions);
            Assert.Contains("delete_users", permissions);
            Assert.Contains("update_roles", permissions);
        }

        [Fact]
        public void TestGetUserPermissions_UserRole()
        {
            var user = new User { Role = UserRole.User };
            var permissions = _authorizationService.GetUserPermissions(user);

            Assert.Equal(2, permissions.Count);
            Assert.Contains("read_profile", permissions);
            Assert.Contains("update_profile", permissions);
        }

        [Fact]
        public void TestGetUserPermissions_NullUser()
        {
            var permissions = _authorizationService.GetUserPermissions(null);
            Assert.Empty(permissions);
        }

        [Fact]
        public void TestHasPermission_EmptyPermission()
        {
            var admin = new User { Role = UserRole.Admin };
            Assert.False(_authorizationService.HasPermission(admin, ""));
            Assert.False(_authorizationService.HasPermission(admin, null!));
        }

        [Fact]
        public void TestRoleHierarchy_AdminHasMorePermissionsThanUser()
        {
            var adminPermissions = _authorizationService.GetUserPermissions(new User { Role = UserRole.Admin });
            var userPermissions = _authorizationService.GetUserPermissions(new User { Role = UserRole.User });

            Assert.True(adminPermissions.Count > userPermissions.Count);

            // All user permissions should be included in admin permissions
            foreach (var permission in userPermissions)
            {
                Assert.Contains(permission, adminPermissions);
            }
        }
    }
}