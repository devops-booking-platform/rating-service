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
using RatingService.Infrastructure.Clients;

namespace RatingService.Tests.Integration;

public class AccommodationRatingControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AccommodationRatingControllerIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateAccommodationRating_Returns200_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accommodationId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");
        _client.DefaultRequestHeaders.Add("X-Test-Username", "testuser");
        _client.DefaultRequestHeaders.Add("X-Test-FirstName", "Test");
        _client.DefaultRequestHeaders.Add("X-Test-LastName", "User");

        _factory.AccommodationClientMock
            .Setup(x => x.GetAccommodationInfo(accommodationId, CancellationToken.None))
            .ReturnsAsync(new GetAccommodationResponse
            {
                HostId = hostId,
                Name = "Test Accommodation"
            });

        var request = new AccommodationRatingRequest
        {
            AccommodationId = accommodationId,
            Rating = 5,
            Comment = "Great place!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/accommodation-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _factory.EventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<AccommodationRatedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAccommodationRating_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var request = new AccommodationRatingRequest
        {
            AccommodationId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great place!"
        };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/accommodation-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAccommodationRating_Returns200_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accommodationId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingRating = new AccommodationRating
            {
                Id = ratingId,
                AccommodationId = accommodationId,
                GuestId = userId,
                Rating = 3,
                Comment = "Old comment",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow,
                LastChangedAt = DateTimeOffset.UtcNow
            };
            context.Set<AccommodationRating>().Add(existingRating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");
        _client.DefaultRequestHeaders.Add("X-Test-Username", "testuser");
        _client.DefaultRequestHeaders.Add("X-Test-FirstName", "Test");
        _client.DefaultRequestHeaders.Add("X-Test-LastName", "User");

        _factory.AccommodationClientMock
            .Setup(x => x.GetAccommodationInfo(accommodationId, CancellationToken.None))
            .ReturnsAsync(new GetAccommodationResponse
            {
                HostId = hostId,
                Name = "Test Accommodation"
            });

        var request = new AccommodationRatingRequest
        {
            Id = ratingId,
            AccommodationId = accommodationId,
            Rating = 5,
            Comment = "Updated comment!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/accommodation-ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database update
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedRating = await context.Set<AccommodationRating>()
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            updatedRating.Should().NotBeNull();
            updatedRating!.Rating.Should().Be(5);
            updatedRating.Comment.Should().Be("Updated comment!");
        }
    }

    [Fact]
    public async Task DeleteAccommodationRating_Returns204_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rating = new AccommodationRating
            {
                Id = ratingId,
                AccommodationId = Guid.NewGuid(),
                GuestId = userId,
                Rating = 5,
                Comment = "Test",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<AccommodationRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");

        // Act
        var response = await _client.DeleteAsync($"/api/accommodation-ratings/{ratingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify database deletion
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedRating = await context.Set<AccommodationRating>()
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            deletedRating.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteAccommodationRating_Returns401_WhenNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rating = new AccommodationRating
            {
                Id = ratingId,
                AccommodationId = Guid.NewGuid(),
                GuestId = ownerId,
                Rating = 5,
                Comment = "Test",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<AccommodationRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Add("X-Test-UserId", differentUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Guest");

        // Act
        var response = await _client.DeleteAsync($"/api/accommodation-ratings/{ratingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRatings_Returns200_WithPagedResults()
    {
        // Arrange
        var accommodationId = Guid.NewGuid();

        // Seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            for (int i = 0; i < 5; i++)
            {
                var rating = new AccommodationRating
                {
                    Id = Guid.NewGuid(),
                    AccommodationId = accommodationId,
                    GuestId = Guid.NewGuid(),
                    Rating = i + 1,
                    Comment = $"Comment {i}",
                    GuestFirstName = "Test",
                    GuestLastName = "User",
                    GuestUsername = $"user{i}",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-i),
                    LastChangedAt = DateTimeOffset.UtcNow.AddDays(-i)
                };
                context.Set<AccommodationRating>().Add(rating);
            }
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/accommodation-ratings?AccommodationId={accommodationId}&Page=1&PageSize=3");

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
            var rating = new AccommodationRating
            {
                Id = ratingId,
                AccommodationId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great!",
                GuestFirstName = "Test",
                GuestLastName = "User",
                GuestUsername = "testuser",
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.Set<AccommodationRating>().Add(rating);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/accommodation-ratings/{ratingId}");

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
        var response = await _client.GetAsync($"/api/accommodation-ratings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
