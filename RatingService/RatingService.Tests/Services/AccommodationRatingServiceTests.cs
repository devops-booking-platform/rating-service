using AutoMapper;
using FluentAssertions;
using Moq;
using RatingService.Common.Events;
using RatingService.Common.Events.Published;
using RatingService.Common.Exceptions;
using RatingService.Domain;
using RatingService.Domain.DTOs;
using RatingService.Infrastructure.Clients;
using RatingService.Repositories.Interfaces;
using RatingService.Services;
using RatingService.Services.Interfaces;

namespace RatingService.Tests.Services;

public class AccommodationRatingServiceTests
{
    private readonly Mock<IRepository<AccommodationRating>> _accommodationRatingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IAccommodationClient> _accommodationClientMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly AccommodationRatingService _service;

    public AccommodationRatingServiceTests()
    {
        _accommodationRatingRepositoryMock = new Mock<IRepository<AccommodationRating>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _accommodationClientMock = new Mock<IAccommodationClient>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _eventBusMock = new Mock<IEventBus>();

        _service = new AccommodationRatingService(
            _accommodationRatingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _accommodationClientMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _eventBusMock.Object
        );
    }

    [Fact]
    public async Task CreateOrUpdateAccommodationRating_CreateNew_WhenIdIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accommodationId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var request = new AccommodationRatingRequest
        {
            Id = null,
            AccommodationId = accommodationId,
            Rating = 5,
            Comment = "Great place!"
        };

        var accommodation = new GetAccommodationResponse
        {
            HostId = hostId,
            Name = "Test Accommodation"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.FirstName).Returns("John");
        _currentUserServiceMock.Setup(x => x.LastName).Returns("Doe");
        _currentUserServiceMock.Setup(x => x.Username).Returns("johndoe");
        _accommodationClientMock.Setup(x => x.GetAccommodationInfo(accommodationId, CancellationToken.None))
            .ReturnsAsync(accommodation);

        // Act
        await _service.CreateOrUpdateAccommodationRating(request);

        // Assert
        _accommodationRatingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AccommodationRating>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<AccommodationRatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateAccommodationRating_Update_WhenIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();
        var accommodationId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        var existingRating = new AccommodationRating
        {
            Id = ratingId,
            AccommodationId = accommodationId,
            GuestId = userId,
            Rating = 3,
            Comment = "Old comment"
        };

        var request = new AccommodationRatingRequest
        {
            Id = ratingId,
            AccommodationId = accommodationId,
            Rating = 5,
            Comment = "Updated comment"
        };

        var accommodation = new GetAccommodationResponse
        {
            HostId = hostId,
            Name = "Test Accommodation"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.FirstName).Returns("John");
        _currentUserServiceMock.Setup(x => x.LastName).Returns("Doe");
        _currentUserServiceMock.Setup(x => x.Username).Returns("johndoe");
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(existingRating);
        _accommodationClientMock.Setup(x => x.GetAccommodationInfo(accommodationId, CancellationToken.None))
            .ReturnsAsync(accommodation);

        // Act
        await _service.CreateOrUpdateAccommodationRating(request);

        // Assert
        _accommodationRatingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AccommodationRating>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<AccommodationRatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        existingRating.Rating.Should().Be(5);
        existingRating.Comment.Should().Be("Updated comment");
    }

    [Fact]
    public async Task CreateOrUpdateAccommodationRating_ThrowsUnauthorized_WhenUserIdIsNull()
    {
        // Arrange
        var request = new AccommodationRatingRequest
        {
            AccommodationId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CreateOrUpdateAccommodationRating(request));
    }

    [Fact]
    public async Task CreateOrUpdateAccommodationRating_ThrowsNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var request = new AccommodationRatingRequest
        {
            Id = ratingId,
            AccommodationId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((AccommodationRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateOrUpdateAccommodationRating(request));
    }

    [Fact]
    public async Task DeleteAccommodationRating_Success_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var rating = new AccommodationRating
        {
            Id = ratingId,
            GuestId = userId,
            AccommodationId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(rating);

        // Act
        await _service.DeleteAccommodationRating(ratingId);

        // Assert
        _accommodationRatingRepositoryMock.Verify(x => x.Remove(rating), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccommodationRating_ThrowsUnauthorized_WhenUserIdIsNull()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteAccommodationRating(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAccommodationRating_ThrowsNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((AccommodationRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAccommodationRating(ratingId));
    }

    [Fact]
    public async Task DeleteAccommodationRating_ThrowsUnauthorized_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var rating = new AccommodationRating
        {
            Id = ratingId,
            GuestId = differentUserId,
            AccommodationId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(rating);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteAccommodationRating(ratingId));
    }

    [Fact]
    public async Task GetRating_Success_WhenRatingExists()
    {
        // Arrange
        var ratingId = Guid.NewGuid();
        var rating = new AccommodationRating
        {
            Id = ratingId,
            AccommodationId = Guid.NewGuid(),
            GuestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!"
        };

        var expectedResponse = new GetRatingResponse
        {
            Id = ratingId,
            Rating = 5,
            Comment = "Great!"
        };

        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(rating);
        _mapperMock.Setup(x => x.Map<GetRatingResponse>(rating))
            .Returns(expectedResponse);

        // Act
        var result = await _service.GetRating(ratingId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetRating_ThrowsNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        var ratingId = Guid.NewGuid();
        _accommodationRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((AccommodationRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetRating(ratingId));
    }
}
