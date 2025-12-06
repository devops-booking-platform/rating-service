using AutoMapper;
using RatingService.Domain.DTOs;

namespace RatingService.Domain.Mappings;

public class RatingMappingProfile : Profile
{
    public RatingMappingProfile()
    {
        CreateMap<HostRating, RatingResponse>()
            .ForMember(dest => dest.GuestFullName,
                opt => opt.MapFrom(src => $"{src.GuestFirstName} {src.GuestLastName}"));
        CreateMap<HostRating, GetRatingResponse>()
            .ForMember(dest => dest.GuestFullName,
                opt => opt.MapFrom(src => $"{src.GuestFirstName} {src.GuestLastName}"));
        
        CreateMap<AccommodationRating, RatingResponse>()
            .ForMember(dest => dest.GuestFullName,
                opt => opt.MapFrom(src => $"{src.GuestFirstName} {src.GuestLastName}"));
        
        CreateMap<AccommodationRating, GetRatingResponse>()
            .ForMember(dest => dest.GuestFullName,
                opt => opt.MapFrom(src => $"{src.GuestFirstName} {src.GuestLastName}"));
    }
}