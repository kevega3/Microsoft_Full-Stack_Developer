using System.Collections.Concurrent;
using SafeVault.Models;

namespace SafeVault.Data
{
    public class InMemoryDatabaseHelper : IDatabaseHelper
    {
        private readonly ConcurrentDictionary<string, User> _usersByUsername = new();
        private readonly ConcurrentDictionary<string, User> _usersByEmail = new();
        private readonly ConcurrentDictionary<int, User> _usersById = new();
        private int _nextId = 1;

        public void InitializeDatabase()
        {
            // No-op for in-memory database
        }

        public User? GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            _usersByUsername.TryGetValue(username, out var user);
            return user;
        }

        public User? GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            _usersByEmail.TryGetValue(email, out var user);
            return user;
        }

        public bool CreateUser(User user)
        {
            if (user == null)
                return false;

            if (_usersByUsername.ContainsKey(user.Username))
                return false;

            if (_usersByEmail.ContainsKey(user.Email))
                return false;

            user.UserID = _nextId++;
            _usersByUsername[user.Username] = user;
            _usersByEmail[user.Email] = user;
            _usersById[user.UserID] = user;
            return true;
        }

        public bool UpdateUser(User user)
        {
            if (user == null)
                return false;

            if (!_usersById.ContainsKey(user.UserID))
                return false;

            _usersById[user.UserID] = user;
            _usersByUsername[user.Username] = user;
            _usersByEmail[user.Email] = user;
            return true;
        }

        public bool DeleteUser(int userId)
        {
            if (!_usersById.TryGetValue(userId, out var user))
                return false;

            _usersById.TryRemove(userId, out _);
            _usersByUsername.TryRemove(user.Username, out _);
            _usersByEmail.TryRemove(user.Email, out _);
            return true;
        }

        public List<User> GetAllUsers()
        {
            return _usersById.Values.ToList();
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            return _usersByUsername.ContainsKey(username);
        }
    }
}