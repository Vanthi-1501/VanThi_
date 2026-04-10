namespace VanThi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }

        public Booking Booking { get; set; }
    }
}