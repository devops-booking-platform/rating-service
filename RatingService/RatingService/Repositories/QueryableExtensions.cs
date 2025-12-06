using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RatingService.Domain.DTOs;
using AutoMapper.QueryableExtensions;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace RatingService.Repositories;

public static class QueryableExtensions
{
    public static async Task<PagedResult<TResult>> ToPagedAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        int page,
        int pageSize,
        IConfigurationProvider mapperConfig,
        Expression<Func<TSource, int>>? ratingSelector = null)
    {
        var totalCount = await query.CountAsync();
        
        double? average = null;
        if (ratingSelector != null && totalCount > 0)
        {
            average = await query.AverageAsync(ratingSelector);
        }

        var items = await query
            .ProjectTo<TResult>(mapperConfig)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TResult>
        {
            Items = items,
            TotalCount = totalCount,
            PageSize = pageSize,
            Page = page,
            AverageRating = average
        };
    }
}