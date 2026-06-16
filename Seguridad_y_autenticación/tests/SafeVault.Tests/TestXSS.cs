using Xunit;
using SafeVault.Security;

namespace SafeVault.Tests
{
    public class TestXSS
    {
        [Theory]
        [InlineData("<script>alert('xss')</script>")]
        [InlineData("<script src='http://evil.com/script.js'></script>")]
        [InlineData("<script>document.cookie</script>")]
        public void TestDetectScriptTagXss(string maliciousInput)
        {
            Assert.True(InputValidator.ContainsXssAttack(maliciousInput),
                $"Should detect script tag XSS in: {maliciousInput}");
        }

        [Theory]
        [InlineData("javascript:alert('xss')")]
        [InlineData("JAVASCRIPT:alert(document.cookie)")]
        [InlineData("javascript:void(0)")]
        public void TestDetectJavascriptProtocolXss(string maliciousInput)
        {
            Assert.True(InputValidator.ContainsXssAttack(maliciousInput),
                $"Should detect javascript: protocol XSS in: {maliciousInput}");
        }

        [Theory]
        [InlineData("<img onerror='alert(1)'>")]
        [InlineData("<img src=x onerror=alert(1)>")]
        [InlineData("<body onload='alert(1)'>")]
        [InlineData("<div onclick='alert(1)'>Click</div>")]
        [InlineData("<input onmouseover='alert(1)'>")]
        public void TestDetectEventHandlerXss(string maliciousInput)
        {
            Assert.True(InputValidator.ContainsXssAttack(maliciousInput),
                $"Should detect event handler XSS in: {maliciousInput}");
        }

        [Theory]
        [InlineData("<svg onload='alert(1)'>")]
        [InlineData("<svg/onload=alert(1)>")]
        public void TestDetectSvgXss(string maliciousInput)
        {
            Assert.True(InputValidator.ContainsXssAttack(maliciousInput),
                $"Should detect SVG XSS in: {maliciousInput}");
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("user123")]
        [InlineData("test@example.com")]
        [InlineData("This is a normal comment")]
        public void TestAllowCleanInput(string cleanInput)
        {
            Assert.False(InputValidator.ContainsXssAttack(cleanInput),
                $"Should allow clean input: {cleanInput}");
        }

        [Fact]
        public void TestSanitizeInputRemovesScriptTags()
        {
            var input = "<script>alert('xss')</script>";
            var sanitized = InputValidator.SanitizeInput(input);
            Assert.DoesNotContain("<script>", sanitized);
            Assert.DoesNotContain("</script>", sanitized);
            // The alert text remains but is escaped
            Assert.DoesNotContain("<script>alert", sanitized);
        }

        [Fact]
        public void TestSanitizeInputRemovesEventHandlers()
        {
            var input = "<img onerror='alert(1)' src='image.jpg'>";
            var sanitized = InputValidator.SanitizeInput(input);
            Assert.DoesNotContain("onerror", sanitized);
        }

        [Fact]
        public void TestSanitizeInputEscapesHtmlEntities()
        {
            var input = "Hello & World";
            var sanitized = InputValidator.SanitizeInput(input);
            Assert.Contains("&amp;", sanitized);
        }

        [Fact]
        public void TestXssInUsernameField()
        {
            var maliciousInputs = new[]
            {
                "<script>alert('xss')</script>",
                "javascript:alert(1)",
                "<img onerror='alert(1)'>"
            };

            foreach (var input in maliciousInputs)
            {
                Assert.True(InputValidator.ContainsXssAttack(input),
                    $"XSS in username should be detected: {input}");
            }
        }

        [Fact]
        public void TestXssInEmailField()
        {
            var maliciousInputs = new[]
            {
                "test<script>alert(1)</script>@example.com",
                "javascript:alert(1)@example.com",
                "<img onerror='alert(1)'>@example.com"
            };

            foreach (var input in maliciousInputs)
            {
                Assert.True(InputValidator.ContainsXssAttack(input),
                    $"XSS in email should be detected: {input}");
            }
        }

        [Fact]
        public void TestEncodedXssDetection()
        {
            // Test detection of encoded XSS attempts
            var encodedInputs = new[]
            {
                "%3Cscript%3Ealert('xss')%3C/script%3E",
                "&lt;script&gt;alert('xss')&lt;/script&gt;"
            };

            foreach (var encodedInput in encodedInputs)
            {
                var decodedInput = System.Net.WebUtility.UrlDecode(encodedInput);
                decodedInput = System.Net.WebUtility.HtmlDecode(decodedInput);

                Assert.True(InputValidator.ContainsXssAttack(decodedInput),
                    $"Should detect encoded XSS: {encodedInput}");
            }
        }

        [Fact]
        public void TestMixedXssAndSqlInjection()
        {
            // Test attacks that combine XSS and SQL injection
            var mixedAttacks = new[]
            {
                "<script>alert('xss')</script>'; DROP TABLE Users--",
                "'; DROP TABLE Users--<script>alert(1)</script>",
                "' OR 1=1--<img onerror='alert(1)'>"
            };

            foreach (var attack in mixedAttacks)
            {
                Assert.True(InputValidator.ContainsXssAttack(attack) ||
                           InputValidator.ContainsSqlInjection(attack),
                    $"Mixed attack should be detected: {attack}");
            }
        }

        [Fact]
        public void TestNullAndEmptyInput()
        {
            Assert.False(InputValidator.ContainsXssAttack(null));
            Assert.False(InputValidator.ContainsXssAttack(string.Empty));
            Assert.False(InputValidator.ContainsSqlInjection(null));
            Assert.False(InputValidator.ContainsSqlInjection(string.Empty));
        }
    }
}