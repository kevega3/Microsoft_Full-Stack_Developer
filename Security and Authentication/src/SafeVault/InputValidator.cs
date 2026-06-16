using System.Text.RegularExpressions;

namespace SafeVault.Security
{
    public static class InputValidator
    {
        private static readonly Regex HtmlTagRegex = new Regex("<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex ScriptTagRegex = new Regex("<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex JavaScriptEventRegex = new Regex(@"on\w+\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SqlKeywordRegex = new Regex(@"(\b(DROP|DELETE|INSERT|UPDATE|SELECT|UNION|ALTER|CREATE|EXEC|EXECUTE)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.Trim();

            input = HtmlTagRegex.Replace(input, string.Empty);
            input = ScriptTagRegex.Replace(input, string.Empty);
            input = JavaScriptEventRegex.Replace(input, string.Empty);

            input = input.Replace("&", "&amp;");
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("\"", "&quot;");
            input = input.Replace("'", "&#x27;");

            return input;
        }

        public static bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            string decoded = System.Net.WebUtility.HtmlDecode(input);

            if (SqlKeywordRegex.IsMatch(decoded))
                return true;

            if (decoded.Contains("';") || decoded.Contains("\";"))
                return true;

            if (decoded.Contains("--") || decoded.Contains("/*") || decoded.Contains("*/"))
                return true;

            // Detect common SQL injection patterns
            string lowerDecoded = decoded.ToLowerInvariant();
            if (lowerDecoded.Contains("' or '") || lowerDecoded.Contains("' or \""))
                return true;

            if (lowerDecoded.Contains("1'='1") || lowerDecoded.Contains("1\"=\"1"))
                return true;

            if (lowerDecoded.Contains("' and '") || lowerDecoded.Contains("' and \""))
                return true;

            return false;
        }

        public static bool ContainsXssAttack(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            string lowerInput = input.ToLowerInvariant();

            if (lowerInput.Contains("<script"))
                return true;

            if (lowerInput.Contains("javascript:"))
                return true;

            if (lowerInput.Contains("onerror=") || lowerInput.Contains("onload=") ||
                lowerInput.Contains("onclick=") || lowerInput.Contains("onmouseover="))
                return true;

            if (lowerInput.Contains("<img") && lowerInput.Contains("onerror"))
                return true;

            if (lowerInput.Contains("<svg") && lowerInput.Contains("onload"))
                return true;

            return false;
        }

        public static string ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be empty.");

            username = username.Trim();

            if (username.Length < 3 || username.Length > 50)
                throw new ArgumentException("Username must be between 3 and 50 characters.");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                throw new ArgumentException("Username can only contain letters, numbers, and underscores.");

            if (ContainsSqlInjection(username))
                throw new ArgumentException("Username contains invalid characters.");

            return username;
        }

        public static string ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be empty.");

            email = email.Trim();

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(email))
                throw new ArgumentException("Invalid email format.");

            if (ContainsSqlInjection(email))
                throw new ArgumentException("Email contains invalid characters.");

            return email;
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.");

            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                throw new ArgumentException("Password must contain at least one uppercase letter.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                throw new ArgumentException("Password must contain at least one lowercase letter.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                throw new ArgumentException("Password must contain at least one digit.");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]"))
                throw new ArgumentException("Password must contain at least one special character.");

            return password;
        }
    }
}