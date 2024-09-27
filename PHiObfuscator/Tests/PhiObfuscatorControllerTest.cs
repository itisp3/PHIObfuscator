using Microsoft.AspNetCore.Mvc;
using Moq;
using PHiObfuscator.Controllers;
using System.Text;


namespace PHiObfuscator.Tests
{
    public class PhiObfuscatorControllerTests
    {
        private readonly PhiObfuscatorController _controller;

        public PhiObfuscatorControllerTests()
        {
            _controller = new PhiObfuscatorController();
        }

        [Fact]
        public async Task PhiObfuscator_NoFile_ReturnsRequest()
        {
            // Act
            var result = await _controller.PhiObfuscator(null);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("No file uploaded", ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task PhiObfuscator_BadFileType_ReturnsRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1);

            // Act
            var result = await _controller.PhiObfuscator(fileMock.Object);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Please upload .TXT files only", ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task PhiObfuscator_ValidFile_ReturnsFileResult()
        {
            // Arrange
            var fileContent = "name: John Doe\naddress: 123 Main St\n";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns((long)fileContent.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)));

            // Act
            var result = await _controller.PhiObfuscator(fileMock.Object);

            // Assert

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("test_sanitized.txt obfuscated successfully", ((OkObjectResult)result).Value);
        }

        [Theory]
        [InlineData("name: John Doe", "name : [REDACTED]")]
        [InlineData("address: 123 Main St", "address : [REDACTED]")]
        [InlineData("dob: 1990-01-01", "dob : [REDACTED]")]
        [InlineData("birth: 1985-05-05", "birth : [REDACTED]")]
        public void Obfuscate_WhenFirstElementContainsSensitiveInfo_ShouldReturnObfuscated(string input, string expected)
        {
            var result = _controller.LineObfuscator(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("password: mypassword", "password: mypassword")]
        [InlineData("data: 12345", "data: 12345")]
        public void Obfuscate_WhenFirstElementDoesNotContainSensitiveInfo_ShouldReturnOriginal(string input, string expected)
        {
            var result = _controller.LineObfuscator(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("email: user@example.com", "email : [REDACTED]")]
        [InlineData("ssn: 123-45-6789", "ssn : [REDACTED]")]
        [InlineData("phone: (123) 456-7890", "phone : [REDACTED]")]
        [InlineData("date: 2023-09-26", "date : [REDACTED]")]
        [InlineData("ip: 192.168.1.1", "ip : [REDACTED]")]
        public void Obfuscate_WhenSecondElementMatchesPattern_ShouldReturnObfuscated(string input, string expected)
        {
            var result = _controller.LineObfuscator(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Obfuscate_WhenInputDoesNotContainColon_ShouldReturnOriginal()
        {
            var input = "noColonInput";
            var expected = "noColonInput";

            var result = _controller.LineObfuscator(input);
            Assert.Equal(expected, result);
        }
    }
}
