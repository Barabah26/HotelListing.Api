using AutoMapper;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Domain;

namespace HotelListing.Api.Application.MappingProfiles;

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, GetCountryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.CountryId));
        CreateMap<Country, GetCountriesDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.CountryId));
        CreateMap<CreateCountryDto, Country>();
        CreateMap<Country, UpdateCountryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.CountryId))
            .ReverseMap()
            .ForMember(d => d.CountryId, opt => opt.MapFrom(s => s.Id));
    }
}