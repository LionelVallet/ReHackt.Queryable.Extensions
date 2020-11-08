using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReHackt.Queryable.Extensions.Tests
{
    public class Email
    {
        private readonly string _email;
        public Email(string email)
        {
            if (!email.Contains("@")) throw new ArgumentException("An e-mail must contain the @ character.");

            _email = email;
        }

        public static implicit operator string(Email email) => email._email;

        public override string ToString() => _email;
    }
}
