using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Security;

namespace SafeVault.Authentication
{
    public class AuthService
    {
        private readonly IDatabaseHelper _databaseHelper;

        public AuthService(IDatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.");

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            try
            {
                var sanitizedUsername = InputValidator.SanitizeInput(username);
                var user = _databaseHelper.GetUserByUsername(sanitizedUsername);

                if (user == null)
                    return null;

                if (VerifyPassword(password, user.PasswordHash))
                    return user;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool Register(CreateUserRequest request)
        {
            if (request == null)
                return false;

            try
            {
                var sanitizedUsername = InputValidator.ValidateUsername(request.Username);
                var sanitizedEmail = InputValidator.ValidateEmail(request.Email);
                InputValidator.ValidatePassword(request.Password);

                if (_databaseHelper.UserExists(sanitizedUsername))
                    return false;

                var user = new User
                {
                    Username = sanitizedUsername,
                    Email = sanitizedEmail,
                    PasswordHash = HashPassword(request.Password),
                    Role = request.Role
                };

                return _databaseHelper.CreateUser(user);
            }
            catch
            {
                return false;
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
                return false;

            var user = Authenticate(username, oldPassword);
            if (user == null)
                return false;

            try
            {
                InputValidator.ValidatePassword(newPassword);
            }
            catch
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            return _databaseHelper.UpdateUser(user);
        }
    }
}