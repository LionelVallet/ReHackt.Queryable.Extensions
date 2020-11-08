using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReHackt.Queryable.Extensions.Tests
{
    public class User
    {
        public string FirstName1 { get; set; }
        public string LastName { get; set; }
        public Email Email { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public long Score { get; set; }
    }
}
