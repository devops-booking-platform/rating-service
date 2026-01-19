using AutoMapper;
using FluentAssertions;
using Moq;
using RatingService.Common.Events;
using RatingService.Common.Events.Published;
using RatingService.Common.Exceptions;
using RatingService.Domain;
using RatingService.Domain.DTOs;
using RatingService.Repositories.Interfaces;
using RatingService.Services;
using RatingService.Services.Interfaces;

namespace RatingService.Tests.Services;

public class HostRatingServiceTests
{
    private readonly Mock<IRepository<HostRating>> _hostRatingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly HostRatingService _service;

    public HostRatingServiceTests()
    {
        _hostRatingRepositoryMock = new Mock<IRepository<HostRating>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _mapperMock = new Mock<IMapper>();

        _service = new HostRatingService(
            _hostRatingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _eventBusMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateOrUpdateHostRating_CreateNew_WhenIdIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var request = new HostRatingRequest
        {
            Id = null,
            HostId = hostId,
            Rating = 5,
            Comment = "Great host!"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.FirstName).Returns("Jane");
        _currentUserServiceMock.Setup(x => x.LastName).Returns("Smith");
        _currentUserServiceMock.Setup(x => x.Username).Returns("janesmith");

        // Act
        await _service.CreateOrUpdateHostRating(request);

        // Assert
        _hostRatingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HostRating>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<HostRatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateHostRating_Update_WhenIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        var existingRating = new HostRating
        {
            Id = ratingId,
            HostId = hostId,
            GuestId = userId,
            Rating = 3,
            Comment = "Old comment"
        };

        var request = new HostRatingRequest
        {
            Id = ratingId,
            HostId = hostId,
            Rating = 5,
            Comment = "Updated comment"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.FirstName).Returns("Jane");
        _currentUserServiceMock.Setup(x => x.LastName).Returns("Smith");
        _currentUserServiceMock.Setup(x => x.Username).Returns("janesmith");
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(existingRating);

        // Act
        await _service.CreateOrUpdateHostRating(request);

        // Assert
        _hostRatingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HostRating>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<HostRatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        existingRating.Rating.Should().Be(5);
        existingRating.Comment.Should().Be("Updated comment");
    }

    [Fact]
    public async Task CreateOrUpdateHostRating_ThrowsUnauthorized_WhenUserIdIsNull()
    {
        // Arrange
        var request = new HostRatingRequest
        {
            HostId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CreateOrUpdateHostRating(request));
    }

    [Fact]
    public async Task CreateOrUpdateHostRating_ThrowsNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var request = new HostRatingRequest
        {
            Id = ratingId,
            HostId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((HostRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateOrUpdateHostRating(request));
    }

    [Fact]
    public async Task DeleteHostRating_Success_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var rating = new HostRating
        {
            Id = ratingId,
            GuestId = userId,
            HostId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(rating);

        // Act
        await _service.DeleteHostRating(ratingId);

        // Assert
        _hostRatingRepositoryMock.Verify(x => x.Remove(rating), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteHostRating_ThrowsUnauthorized_WhenUserIdIsNull()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteHostRating(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteHostRating_ThrowsNotFound_WhenRatingDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((HostRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteHostRating(ratingId));
    }

    [Fact]
    public async Task DeleteHostRating_ThrowsUnauthorized_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var ratingId = Guid.NewGuid();

        var rating = new HostRating
        {
            Id = ratingId,
            GuestId = differentUserId,
            HostId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync(rating);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteHostRating(ratingId));
    }

    [Fact]
    public async Task GetRating_Success_WhenRatingExists()
    {
        // Arrange
        var ratingId = Guid.NewGuid();
        var rating = new HostRating
        {
            Id = ratingId,
            HostId = Guid.NewGuid(),
            GuestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great host!"
        };

        var expectedResponse = new GetRatingResponse
        {
            Id = ratingId,
            Rating = 5,
            Comment = "Great host!"
        };

        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
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
        _hostRatingRepositoryMock.Setup(x => x.GetByIdAsync(ratingId))
            .ReturnsAsync((HostRating?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetRating(ratingId));
    }
}
