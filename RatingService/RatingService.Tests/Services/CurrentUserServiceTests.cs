using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RatingService.Services;

namespace RatingService.Tests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public void UserId_ReturnsCorrectValue_WhenClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void Role_ReturnsCorrectValue_WhenClaimExists()
    {
        // Arrange
        var role = "Admin";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.Role;

        // Assert
        result.Should().Be(role);
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Username_ReturnsCorrectValue_WhenClaimExists()
    {
        // Arrange
        var username = "testuser";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.Username;

        // Assert
        result.Should().Be(username);
    }

    [Fact]
    public void FirstName_ReturnsCorrectValue_WhenClaimExists()
    {
        // Arrange
        var firstName = "John";
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, firstName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.FirstName;

        // Assert
        result.Should().Be(firstName);
    }

    [Fact]
    public void LastName_ReturnsCorrectValue_WhenClaimExists()
    {
        // Arrange
        var lastName = "Doe";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Surname, lastName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.LastName;

        // Assert
        result.Should().Be(lastName);
    }

    [Fact]
    public void Properties_ReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        service.Role.Should().BeNull();
        service.Username.Should().BeNull();
        service.FirstName.Should().BeNull();
        service.LastName.Should().BeNull();
        service.IsAuthenticated.Should().BeFalse();
    }
}
