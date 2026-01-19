using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RatingService.Common.Events.Published;
using RatingService.Data;
using RatingService.Domain;
using RatingService.Domain.DTOs;

namespace RatingService.Tests.Integration;

public class HostRatingControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HostRatingControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.AuthenticationScheme, options => { });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CreateHostRating_Returns201_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");
        _client.DefaultRequestHeaders.Add("X-Test-Username", "testuser");
        _client.DefaultRequestHeaders.Add("X-Test-FirstName", "Test");
        _client.DefaultRequestHeaders.Add("X-Test-LastName", "User");

        var request = new HostRatingRequest
        {
            HostId = hostId,
            Rating = 5,
            Comment = "Great host!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/host-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.EventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<HostRatedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateHostRating_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var request = new HostRatingRequest
        {
            HostId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great host!"
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/host-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateHostRating_Returns201_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingRating = new HostRating
            {
                Id = ratingId,
                HostId = hostId,
                GuestId = userId,
                Rating = 3,
                Comment = "Old comment",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow,
                LastChangedAt = DateTimeOffset.UtcNow
            };
            context.Set<HostRating>().Add(existingRating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");
        _client.DefaultRequestHeaders.Add("X-Test-Username", "testuser");
        _client.DefaultRequestHeaders.Add("X-Test-FirstName", "Test");
        _client.DefaultRequestHeaders.Add("X-Test-LastName", "User");

        var request = new HostRatingRequest
        {
            Id = ratingId,
            HostId = hostId,
            Rating = 5,
            Comment = "Updated comment!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/host-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify database update
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedRating = await context.Set<HostRating>()
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            updatedRating.Should().NotBeNull();
            updatedRating!.Rating.Should().Be(5);
            updatedRating.Comment.Should().Be("Updated comment!");
        }
    }

    [Fact]
    public async Task DeleteHostRating_Returns201_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rating = new HostRating
            {
                Id = ratingId,
                HostId = Guid.NewGuid(),
                GuestId = userId,
                Rating = 5,
                Comment = "Test",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<HostRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");

        // Act
        var response = await _client.DeleteAsync($"/api/host-ratings/{ratingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify database deletion
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedRating = await context.Set<HostRating>()
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            deletedRating.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteHostRating_Returns401_WhenNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rating = new HostRating
            {
                Id = ratingId,
                HostId = Guid.NewGuid(),
                GuestId = ownerId,
                Rating = 5,
                Comment = "Test",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<HostRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", differentUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");

        // Act
        var response = await _client.DeleteAsync($"/api/host-ratings/{ratingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRatings_Returns200_WithPagedResults()
    {
        // Arrange
        var hostId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            for (int i = 0; i < 5; i++)
            {
                var rating = new HostRating
                {
                    Id = Guid.NewGuid(),
                    HostId = hostId,
                    GuestId = Guid.NewGuid(),
                    Rating = i + 1,
                    Comment = $"Comment {i}",
                    GuestFirstName = "Test",
                    GuestLastName = "User",
                    GuestUsername = $"user{i}",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-i),
                    LastChangedAt = DateTimeOffset.UtcNow.AddDays(-i)
                };
                context.Set<HostRating>().Add(rating);
            }
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/host-ratings?HostId={hostId}&Page=1&PageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RatingResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetRating_Returns200_WhenRatingExists()
    {
        // Arrange
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rating = new HostRating
            {
                Id = ratingId,
                HostId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great host!",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<HostRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/host-ratings/{ratingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetRatingResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(ratingId);
        result.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetRating_Returns404_WhenRatingDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/host-ratings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateHostRating_Returns400_WhenInvalidRating()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");
        _client.DefaultRequestHeaders.Add("X-Test-Username", "testuser");

        var request = new HostRatingRequest
        {
            HostId = Guid.NewGuid(),
            Rating = 10, // Invalid rating
            Comment = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/host-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
