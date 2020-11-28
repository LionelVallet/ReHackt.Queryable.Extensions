using System.Collections.Generic;

namespace ReHackt.Queryable.AutoMapperExtensions.UnitTests.Models
{
    class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}
