using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ReHackt.Queryable.Extensions;
using System.Linq.Expressions;

namespace ReHackt.Queryable.Extensions.Tests
{
    public class Tests
    {
        readonly User Hubert = new User
        {
            FirstName1 = "Hubert",
            LastName = "Bonisseur de La Bath",
            Email = new Email("hubert@bonisseur.fr"),
            Birthday = new DateTimeOffset(1972, 02, 01, 0, 0, 0, TimeSpan.FromHours(2)),
            Score = 186000
        };

        readonly User Pierre = new User
        {
            FirstName1 = "Pierre",
            LastName = "Durand",
            Email = new Email("durant@example.com"),
            Birthday = new DateTimeOffset(2000, 01, 01, 00, 00, 00, TimeSpan.Zero),
            Score = 120
        };

        [Test]
        public void Filter_on_property_ending_with_number_works()
        {
            // arrange
            var users = new List<User> { Hubert, Pierre };

            // act
            var result = users.AsQueryable().Filter("FirstName1 eq \"Hubert\"");

            // assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void Filter_on_value_type_property_works()
        {
            // arrange
            var users = new List<User> { Hubert, Pierre }.AsQueryable();
            var query = "Email eq \"durant@example.com\"";

            // act
            var result = users.Filter(query);

            // assert
            Assert.AreEqual(1, result.Count());
        }
    }
}