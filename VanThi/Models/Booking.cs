using VanThi.Models;

namespace VanThi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Room? Room { get; set; }
        public Customer? Customer { get; set; }
    }
}