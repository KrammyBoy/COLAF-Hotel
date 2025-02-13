namespace COLAFHotel.Models
{
    public class Payment
    {
        public string PaymentMethod { get; set; }  // e.g., Credit Card, Debit Card, etc.
        public decimal Amount { get; set; }
    }
}
