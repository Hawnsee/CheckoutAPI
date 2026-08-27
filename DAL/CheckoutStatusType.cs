namespace DAL
{
    public enum CheckoutStatusType : byte
    {
        CREATED,
        PROCESSING_PAYMENT,
        PAYMENT_PROCESSED,
        CANCELLED,
    }
}