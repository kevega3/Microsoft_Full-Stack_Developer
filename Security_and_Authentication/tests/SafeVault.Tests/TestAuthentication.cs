using Xunit;
using SafeVault.Authentication;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Security;

namespace SafeVault.Tests
{
    public class TestAuthentication
    {
        private readonly AuthService _authService;
        private readonly InMemoryDatabaseHelper _databaseHelper;

        public TestAuthentication()
        {
            // Use in-memory database for testing
            _databaseHelper = new InMemoryDatabaseHelper();
            _databaseHelper.InitializeDatabase();
            _authService = new AuthService(_databaseHelper);
        }

        [Fact]
        public void TestHashPassword_GeneratesHash()
        {
            var password = "SecurePass123!";
            var hash = _authService.HashPassword(password);

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void TestHashPassword_GeneratesUniqueHashes()
        {
            var password = "SecurePass123!";
            var hash1 = _authService.HashPassword(password);
            var hash2 = _authService.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void TestVerifyPassword_CorrectPassword()
        {
            var password = "SecurePass123!";
            var hash = _authService.HashPassword(password);

            Assert.True(_authService.VerifyPassword(password, hash));
        }

        [Fact]
        public void TestVerifyPassword_WrongPassword()
        {
            var password = "SecurePass123!";
            var hash = _authService.HashPassword(password);

            Assert.False(_authService.VerifyPassword("WrongPass123!", hash));
        }

        [Fact]
        public void TestVerifyPassword_EmptyInputs()
        {
            Assert.False(_authService.VerifyPassword("", "hash"));
            Assert.False(_authService.VerifyPassword("password", ""));
            Assert.False(_authService.VerifyPassword("", ""));
        }

        [Fact]
        public void TestRegister_SuccessfulRegistration()
        {
            var request = new CreateUserRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            var result = _authService.Register(request);
            Assert.True(result);

            // Verify user was created
            var user = _databaseHelper.GetUserByUsername("newuser");
            Assert.NotNull(user);
            Assert.Equal("newuser@example.com", user.Email);
            Assert.Equal(UserRole.User, user.Role);
        }

        [Fact]
        public void TestRegister_DuplicateUsername()
        {
            var request1 = new CreateUserRequest
            {
                Username = "testuser",
                Email = "test1@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            var request2 = new CreateUserRequest
            {
                Username = "testuser",
                Email = "test2@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            Assert.True(_authService.Register(request1));
            Assert.False(_authService.Register(request2));
        }

        [Fact]
        public void TestRegister_InvalidUsername()
        {
            var request = new CreateUserRequest
            {
                Username = "ab", // Too short
                Email = "test@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            Assert.False(_authService.Register(request));
        }

        [Fact]
        public void TestRegister_InvalidEmail()
        {
            var request = new CreateUserRequest
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            Assert.False(_authService.Register(request));
        }

        [Fact]
        public void TestRegister_WeakPassword()
        {
            var request = new CreateUserRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "weak",
                Role = UserRole.User
            };

            Assert.False(_authService.Register(request));
        }

        [Fact]
        public void TestRegister_SqlInjectionInUsername()
        {
            var request = new CreateUserRequest
            {
                Username = "admin'--",
                Email = "test@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            Assert.False(_authService.Register(request));
        }

        [Fact]
        public void TestRegister_XssInUsername()
        {
            var request = new CreateUserRequest
            {
                Username = "<script>alert('xss')</script>",
                Email = "test@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            Assert.False(_authService.Register(request));
        }

        [Fact]
        public void TestAuthenticate_SuccessfulLogin()
        {
            var request = new CreateUserRequest
            {
                Username = "loginuser",
                Email = "login@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            _authService.Register(request);

            var user = _authService.Authenticate("loginuser", "SecurePass123!");
            Assert.NotNull(user);
            Assert.Equal("loginuser", user.Username);
        }

        [Fact]
        public void TestAuthenticate_WrongPassword()
        {
            var request = new CreateUserRequest
            {
                Username = "loginuser2",
                Email = "login2@example.com",
                Password = "SecurePass123!",
                Role = UserRole.User
            };

            _authService.Register(request);

            var user = _authService.Authenticate("loginuser2", "WrongPass123!");
            Assert.Null(user);
        }

        [Fact]
        public void TestAuthenticate_NonExistentUser()
        {
            var user = _authService.Authenticate("nonexistent", "SecurePass123!");
            Assert.Null(user);
        }

        [Fact]
        public void TestAuthenticate_EmptyCredentials()
        {
            Assert.Null(_authService.Authenticate("", "password"));
            Assert.Null(_authService.Authenticate("username", ""));
            Assert.Null(_authService.Authenticate("", ""));
        }

        [Fact]
        public void TestAuthenticate_SqlInjectionAttempt()
        {
            var user = _authService.Authenticate("admin'--", "password");
            Assert.Null(user);
        }

        [Fact]
        public void TestChangePassword_SuccessfulChange()
        {
            var request = new CreateUserRequest
            {
                Username = "changepass",
                Email = "change@example.com",
                Password = "OldPass123!",
                Role = UserRole.User
            };

            _authService.Register(request);

            var result = _authService.ChangePassword("changepass", "OldPass123!", "NewPass123!");
            Assert.True(result);

            // Verify old password no longer works
            var user = _authService.Authenticate("changepass", "OldPass123!");
            Assert.Null(user);

            // Verify new password works
            user = _authService.Authenticate("changepass", "NewPass123!");
            Assert.NotNull(user);
        }

        [Fact]
        public void TestChangePassword_WrongOldPassword()
        {
            var request = new CreateUserRequest
            {
                Username = "changepass2",
                Email = "change2@example.com",
                Password = "OldPass123!",
                Role = UserRole.User
            };

            _authService.Register(request);

            var result = _authService.ChangePassword("changepass2", "WrongOldPass!", "NewPass123!");
            Assert.False(result);
        }
    }
}