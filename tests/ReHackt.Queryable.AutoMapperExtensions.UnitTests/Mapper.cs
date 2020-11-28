using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using ReHackt.Queryable.AutoMapperExtensions.UnitTests.Models;

namespace ReHackt.Queryable.AutoMapperExtensions.UnitTests
{
    static class Mapper
    {
        private static IMapper _mapper = new AutoMapper.Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddExpressionMapping();
            cfg.AddProfile(typeof(MapperProfile));
        }));
        public static IMapper Instance => _mapper;
    }

    class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dto => dto.Customer, c => c.MapFrom(o => o.Customer.Name))
                .ReverseMap();
        }
    }
}
