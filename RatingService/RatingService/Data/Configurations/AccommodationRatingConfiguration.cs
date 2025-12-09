using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatingService.Domain;

namespace RatingService.Data.Configurations;

public class AccommodationRatingConfiguration : IEntityTypeConfiguration<AccommodationRating>
{
    public void Configure(EntityTypeBuilder<AccommodationRating> builder)
    {
        builder.Property(x => x.GuestId)
            .IsRequired();

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(AccommodationRating.CommentMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.GuestFirstName)
            .HasMaxLength(AccommodationRating.ViewingPropertiesMaxLength);

        builder.Property(x => x.GuestLastName)
            .HasMaxLength(AccommodationRating.ViewingPropertiesMaxLength);

        builder.Property(x => x.GuestUsername)
            .HasMaxLength(AccommodationRating.ViewingPropertiesMaxLength);
    }
}