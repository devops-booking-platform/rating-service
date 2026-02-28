using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RatingService.Data;
using RatingService.Domain;
using RatingService.Repositories;

namespace RatingService.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<AccommodationRating> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new Repository<AccommodationRating>(_context);
    }

    [Fact]
    public async Task AddAsync_AddsEntityToContext()
    {
        // Arrange
        var rating = new AccommodationRating
        {
            Id = Guid.NewGuid(),
            AccommodationId = Guid.NewGuid(),
            GuestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        await _repository.AddAsync(rating);
        await _context.SaveChangesAsync();

        // Assert
        var savedRating = await _context.Set<AccommodationRating>().FindAsync(rating.Id);
        savedRating.Should().NotBeNull();
        savedRating!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleEntitiesToContext()
    {
        // Arrange
        var ratings = new List<AccommodationRating>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccommodationId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great!",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccommodationId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 4,
                Comment = "Good!",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        await _repository.AddRangeAsync(ratings);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.Set<AccommodationRating>().CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        // Arrange
        var ratingId = Guid.NewGuid();
        var rating = new AccommodationRating
        {
            Id = ratingId,
            AccommodationId = Guid.NewGuid(),
            GuestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.Set<AccommodationRating>().AddAsync(rating);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(ratingId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ratingId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Remove_RemovesEntityFromContext()
    {
        // Arrange
        var rating = new AccommodationRating
        {
            Id = Guid.NewGuid(),
            AccommodationId = Guid.NewGuid(),
            GuestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.Set<AccommodationRating>().AddAsync(rating);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(rating);
        await _context.SaveChangesAsync();

        // Assert
        var deletedRating = await _context.Set<AccommodationRating>().FindAsync(rating.Id);
        deletedRating.Should().BeNull();
    }

    [Fact]
    public void Query_ReturnsQueryable()
    {
        // Arrange
        var ratings = new List<AccommodationRating>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccommodationId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great!",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccommodationId = Guid.NewGuid(),
                GuestId = Guid.NewGuid(),
                Rating = 4,
                Comment = "Good!",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _context.Set<AccommodationRating>().AddRange(ratings);
        _context.SaveChanges();

        // Act
        var query = _repository.Query();
        var count = query.Count();

        // Assert
        query.Should().NotBeNull();
        count.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
