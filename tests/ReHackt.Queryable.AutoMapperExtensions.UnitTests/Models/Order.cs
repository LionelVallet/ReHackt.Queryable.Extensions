namespace ReHackt.Queryable.AutoMapperExtensions.UnitTests.Models
{
    class Order
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public Customer Customer { get; set; }
    }
}
