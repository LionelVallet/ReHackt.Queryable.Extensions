using FluentAssertions;
using ReHackt.Queryable.Extensions.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReHackt.Queryable.Extensions.UnitTests
{
    public class QueryableFilterTests
    {
        private readonly IQueryable<User> _users = new List<User>{
            new User
            {
                FirstName1 = "Hubert",
                LastName = "Bonisseur de La Bath",
                Email = new Email("hubert@bonisseur.fr"),
                Birthday = new DateTimeOffset(1972, 02, 01, 0, 0, 0, TimeSpan.FromHours(2)),
                Score = 186000,
                Amount = 56.54,
                Status = Status.Offline
            },
            new User
            {
                FirstName1 = "Pierre",
                LastName = "Durand",
                Email = new Email("durant@example.com"),
                Birthday = new DateTimeOffset(2000, 01, 01, 00, 00, 00, TimeSpan.Zero),
                Score = 120,
                Amount = 8191.03,
                Status = Status.Online
            }
        }.AsQueryable();

        [Fact]
        public void Filter_on_property_ending_with_number_works()
        {
            var result = _users.Filter("FirstName1 eq \"Hubert\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_value_type_property_works()
        {
            var result = _users.Filter("Email eq \"durant@example.com\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_double_property_works()
        {
            var result = _users.Filter("Amount lt 100.5");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_enum_property_works()
        {
            var result = _users.Filter("Status eq \"Online\"");
            result.Count().Should().Be(1);
        }
    }
}