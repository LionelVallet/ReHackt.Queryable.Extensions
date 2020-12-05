using System;
using System.Collections.Generic;

namespace ReHackt.Queryable.Extensions.UnitTests.Models
{
    public class User
    {
        public string FirstName1 { get; set; }
        public string LastName { get; set; }
        public Email Email { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public long Score { get; set; }
        public double Amount { get; set; }
        public Status Status { get; set; }
        public int? NullableInt { get; set; }
        public string String { get; set; }
        public Team Team { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }

    public enum Status
    {
        Offline = 0,
        Online = 1
    }
}
