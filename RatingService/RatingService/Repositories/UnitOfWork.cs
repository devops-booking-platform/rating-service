using RatingService.Data;
using RatingService.Repositories.Interfaces;

namespace RatingService.Repositories;

public class UnitOfWork(
    ApplicationDbContext context)
    : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}