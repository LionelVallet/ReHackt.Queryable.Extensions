using AutoMapper;
using FluentAssertions;
using ReHackt.Queryable.AutoMapperExtensions.UnitTests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReHackt.Queryable.AutoMapperExtensions.UnitTests
{
    public class FilterTests
    {
        private readonly IMapper _mapper = Mapper.Instance;

        private readonly IQueryable<Order> _orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                Code = "O1",
                Customer = new Customer
                {
                    Id = 1,
                    Name = "C1"
                }
            },
            new Order
            {
                Id = 2,
                Code = "O2",
                Customer = new Customer
                {
                    Id = 2,
                    Name = "C2"
                }
            }
        }.AsQueryable();


        [Fact]
        public void Filter_on_dto_works()
        {
            var result = _orders.Filter<OrderDto, Order>(_mapper, "Customer eq \"C1\"");
            result.Count().Should().Be(1);
        }
    }
}
