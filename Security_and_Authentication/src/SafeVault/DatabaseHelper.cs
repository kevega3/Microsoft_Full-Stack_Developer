using System.Data;
using System.Data.SqlClient;
using SafeVault.Models;

namespace SafeVault.Data
{
    public interface IDatabaseHelper
    {
        void InitializeDatabase();
        User? GetUserByUsername(string username);
        User? GetUserByEmail(string email);
        bool CreateUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(int userId);
        List<User> GetAllUsers();
        bool UserExists(string username);
    }

    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void InitializeDatabase()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INT PRIMARY KEY AUTO_INCREMENT,
                    Username VARCHAR(100) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    Role VARCHAR(20) NOT NULL DEFAULT 'User'
                )";

            using var command = new SqlCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public User? GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT UserID, Username, Email, PasswordHash, Role FROM Users WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = Enum.Parse<UserRole>(reader.GetString(4))
                };
            }

            return null;
        }

        public User? GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT UserID, Username, Email, PasswordHash, Role FROM Users WHERE Email = @Email";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = Enum.Parse<UserRole>(reader.GetString(4))
                };
            }

            return null;
        }

        public bool CreateUser(User user)
        {
            if (user == null)
                return false;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@Username, @Email, @PasswordHash, @Role)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Role", user.Role.ToString());

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool UpdateUser(User user)
        {
            if (user == null)
                return false;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "UPDATE Users SET Email = @Email, Role = @Role WHERE UserID = @UserID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", user.UserID);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Role", user.Role.ToString());

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool DeleteUser(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "DELETE FROM Users WHERE UserID = @UserID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", userId);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT UserID, Username, Email, PasswordHash, Role FROM Users";

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = Enum.Parse<UserRole>(reader.GetString(4))
                });
            }

            return users;
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }
}