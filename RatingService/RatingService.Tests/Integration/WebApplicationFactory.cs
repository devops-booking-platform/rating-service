using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using RatingService.Common.Events;
using RatingService.Data;
using RatingService.Infrastructure.Clients;
using StackExchange.Redis;

namespace RatingService.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IEventBus> EventBusMock { get; } = new();
    public Mock<IAccommodationClient> AccommodationClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Replace event bus with mock
            services.RemoveAll(typeof(IEventBus));
            services.AddSingleton(EventBusMock.Object);

            // Replace accommodation client with mock
            services.RemoveAll(typeof(IAccommodationClient));
            services.AddSingleton(AccommodationClientMock.Object);

            // Build the service provider
            var sp = services.BuildServiceProvider();

            services.RemoveAll<IConnectionMultiplexer>();
            var mockRedisDb = new Mock<IDatabase>();
            mockRedisDb.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(false);
            mockRedisDb.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), 
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var mockRedis = new Mock<IConnectionMultiplexer>();
            mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(mockRedisDb.Object);
                
            services.AddSingleton<IConnectionMultiplexer>(mockRedis.Object);
            // Create a scope to get the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}
