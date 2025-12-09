using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatingService.Domain;

namespace RatingService.Data.Configurations;

public class HostRatingConfiguration : IEntityTypeConfiguration<HostRating>
{
    public void Configure(EntityTypeBuilder<HostRating> builder)
    {
        builder.Property(x => x.GuestId)
            .IsRequired();

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(HostRating.CommentMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.GuestFirstName)
            .HasMaxLength(HostRating.ViewingPropertiesMaxLength);

        builder.Property(x => x.GuestLastName)
            .HasMaxLength(HostRating.ViewingPropertiesMaxLength);

        builder.Property(x => x.GuestUsername)
            .HasMaxLength(HostRating.ViewingPropertiesMaxLength);
    }
}