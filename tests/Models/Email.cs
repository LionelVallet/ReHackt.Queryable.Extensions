using System;

namespace ReHackt.Queryable.Extensions.UnitTests.Models
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
