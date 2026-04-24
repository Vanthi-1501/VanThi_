using System;

namespace VanThi.Models
{
    public class Stay
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public DateTime ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }
        public bool IsActive { get; set; } = true;

        public Booking? Booking { get; set; }
        public Room? Room { get; set; }
    }
}
