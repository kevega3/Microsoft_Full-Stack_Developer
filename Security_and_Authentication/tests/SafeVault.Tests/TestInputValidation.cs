using Xunit;
using SafeVault.Security;

namespace SafeVault.Tests
{
    public class TestInputValidation
    {
        [Fact]
        public void TestSanitizeInput_RemovesHtmlTags()
        {
            var input = "<script>alert('xss')</script>";
            var result = InputValidator.SanitizeInput(input);
            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("</script>", result);
        }

        [Fact]
        public void TestSanitizeInput_RemovesEventHandlers()
        {
            var input = "<img onerror='alert(1)' src='x'>";
            var result = InputValidator.SanitizeInput(input);
            Assert.DoesNotContain("onerror", result);
        }

        [Fact]
        public void TestSanitizeInput_EscapesSpecialCharacters()
        {
            var input = "test&example";
            var result = InputValidator.SanitizeInput(input);
            Assert.Contains("&amp;", result);
        }

        [Fact]
        public void TestContainsSqlInjection_DetectsUnionSelect()
        {
            var input = "1' UNION SELECT * FROM Users--";
            Assert.True(InputValidator.ContainsSqlInjection(input));
        }

        [Fact]
        public void TestContainsSqlInjection_DetectsDropTable()
        {
            var input = "'; DROP TABLE Users;--";
            Assert.True(InputValidator.ContainsSqlInjection(input));
        }

        [Fact]
        public void TestContainsSqlInjection_DetectsComments()
        {
            var input = "admin'/**/OR/**/1=1";
            Assert.True(InputValidator.ContainsSqlInjection(input));
        }

        [Fact]
        public void TestContainsSqlInjection_AllowsCleanInput()
        {
            var input = "john_doe";
            Assert.False(InputValidator.ContainsSqlInjection(input));
        }

        [Fact]
        public void TestContainsXssAttack_DetectsScriptTags()
        {
            var input = "<script>alert('xss')</script>";
            Assert.True(InputValidator.ContainsXssAttack(input));
        }

        [Fact]
        public void TestContainsXssAttack_DetectsJavascriptProtocol()
        {
            var input = "javascript:alert('xss')";
            Assert.True(InputValidator.ContainsXssAttack(input));
        }

        [Fact]
        public void TestContainsXssAttack_DetectsOnError()
        {
            var input = "<img onerror='alert(1)'>";
            Assert.True(InputValidator.ContainsXssAttack(input));
        }

        [Fact]
        public void TestContainsXssAttack_AllowsCleanInput()
        {
            var input = "Hello World";
            Assert.False(InputValidator.ContainsXssAttack(input));
        }

        [Fact]
        public void TestValidateUsername_ValidUsername()
        {
            var username = InputValidator.ValidateUsername("john_doe");
            Assert.Equal("john_doe", username);
        }

        [Fact]
        public void TestValidateUsername_ThrowsOnEmpty()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateUsername(""));
        }

        [Fact]
        public void TestValidateUsername_ThrowsOnSpecialChars()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateUsername("john@doe"));
        }

        [Fact]
        public void TestValidateUsername_ThrowsOnSqlInjection()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateUsername("admin'--"));
        }

        [Fact]
        public void TestValidateEmail_ValidEmail()
        {
            var email = InputValidator.ValidateEmail("test@example.com");
            Assert.Equal("test@example.com", email);
        }

        [Fact]
        public void TestValidateEmail_ThrowsOnInvalidFormat()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateEmail("invalid-email"));
        }

        [Fact]
        public void TestValidateEmail_ThrowsOnSqlInjection()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateEmail("test'; DROP TABLE Users--@example.com"));
        }

        [Fact]
        public void TestValidatePassword_ValidPassword()
        {
            var password = InputValidator.ValidatePassword("SecurePass123!");
            Assert.Equal("SecurePass123!", password);
        }

        [Fact]
        public void TestValidatePassword_ThrowsOnTooShort()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidatePassword("Short1!"));
        }

        [Fact]
        public void TestValidatePassword_ThrowsOnNoUppercase()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidatePassword("nouppercase123!"));
        }

        [Fact]
        public void TestValidatePassword_ThrowsOnNoDigit()
        {
            Assert.Throws<ArgumentException>(() => InputValidator.ValidatePassword("NoDigitHere!"));
        }
    }
}