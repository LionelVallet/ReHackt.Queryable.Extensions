using FluentAssertions;
using ReHackt.Queryable.Extensions.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReHackt.Queryable.Extensions.UnitTests
{
    public class FilterTests
    {
        private readonly IQueryable<User> _users = new List<User>
        {
            new User
            {
                FirstName1 = "Hubert",
                LastName = "Bonisseur de La Bath",
                Email = new Email("hubert@bonisseur.fr"),
                Birthday = new DateTimeOffset(1972, 02, 01, 0, 0, 0, TimeSpan.FromHours(2)),
                Score = 186000,
                Amount = 56.54,
                Status = Status.Offline,
                NullableInt = 8,
                String = "string",
                Team = new Team
                {
                    Leader = new User
                    {
                        FirstName1 = "René",
                        LastName = "Coty"
                    },
                    Name = "Frenchies"
                },
                Tags = new List<string>{ "Tag1", "Tag2" }
            },
            new User
            {
                FirstName1 = "James",
                LastName = "Bond",
                Email = new Email("007@mi6.gov.uk"),
                Birthday = new DateTimeOffset(2000, 01, 01, 00, 00, 00, TimeSpan.Zero),
                Score = 120,
                Amount = 8191.03,
                Status = Status.Online,
                NullableInt = null,
                String = null,
                Team = new Team
                {
                    Leader = new User
                    {
                        FirstName1 = "Élisabeth",
                        LastName = "d'York"
                    },
                    Name = "British"
                },
                Tags = new List<string>()
            }
        }.AsQueryable();


        [Fact]
        public void Filter_on_property_ending_with_number_works()
        {
            var result = _users.Filter("FirstName1 eq \"Hubert\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_date_time_property_works()
        {
            var result = _users.Filter("Birthday gt \"1999/01/01\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_double_property_works()
        {
            var result = _users.Filter("Amount lt 100.5");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_enum_property_with_string_works()
        {
            var result = _users.Filter("Status eq \"Online\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_enum_property_with_int_works()
        {
            var result = _users.Filter("Status eq 1");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_with_null_values_works()
        {
            var result = _users.Filter("NullableInt eq null and String eq null");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_nested_property_works()
        {
            var result = _users.Filter("Team.Leader.LastName eq \"Coty\"");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_enumerable_property_works()
        {
            var result = _users.Filter("\"Tag2\" in Tags");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_property_contained_in_array_works()
        {
            var result = _users.Filter("LastName in [\"Bing\", \"Bang\", \"Bond\"]");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_string_property_content_works()
        {
            var result = _users.Filter("\"Bath\" in LastName");
            result.Count().Should().Be(1);
        }

        [Fact]
        public void Filter_on_string_property_contained_in_string_works()
        {
            var result = _users.Filter("LastName in \"Bing Bang Bond\"");
            result.Count().Should().Be(1);
        }
    }
}