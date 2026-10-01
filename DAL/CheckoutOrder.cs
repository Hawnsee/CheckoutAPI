namespace DAL
{
    public class CheckoutOrder()
    {
        public required string Id { get; set; }

        public CheckoutStatusType Status { get; set; }

        public required decimal Price { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Status: {Status}, Price: {Price}";
        }
    }
}