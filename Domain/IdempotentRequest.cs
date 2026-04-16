namespace CheckoutAPI.Domain
{
    public class IdempotentRequest()
    {
        public string Key { get; set; }

        public OrderStatusType StatusType { get; set; }

        public override string ToString() => $"[Key]: {Key}, [StatusType]: {StatusType}";
    }
}