using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagement.API.Data;
using UserManagement.API.Services;
using Shared.Models.DTOs;
using Shared.Models.Entities;
using Microsoft.AspNetCore.Identity.Data;
using Shared.Services.Interfaces;
namespace UserManagement.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        public AuthServiceTests()
        {
            // Настройка in-memory базы данных
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            // Mock конфигурации
            _mockConfiguration = new Mock<IConfiguration>();
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
            jwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSection.Setup(x => x["Audience"]).Returns("TestAudience");

            _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

            _mockLogger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(_context, _mockConfiguration.Object, _mockLogger.Object);
        }
        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
        {
            // Arrange
            var request = new Shared.Models.DTOs.RegisterRequest
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!"
            };
            // Act
            var result = await _authService.RegisterAsync(request);
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(request.Email, result.User.Email);
            Assert.Equal(request.Name, result.User.Name);
        }
    }
}
