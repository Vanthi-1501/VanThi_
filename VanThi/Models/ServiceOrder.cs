namespace VanThi.Models
{
    public class ServiceOrder
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }

        public Service Service { get; set; }
    }
}