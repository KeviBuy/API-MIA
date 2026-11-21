using AutoMapper;
using ЛР3_МіА._АПІшка.models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Driver, DriverDto>().ReverseMap();
        CreateMap<DriverCreateDto, Driver>();
        CreateMap<DriverUpdateDto, Driver>();

        CreateMap<Passenger, PassengerDTO>().ReverseMap();
        CreateMap<PassengerCreateDTO, Passenger>();
    }
}