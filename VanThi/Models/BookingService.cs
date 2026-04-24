using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VanThi.Models
{
    public class BookingService
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int ServiceId { get; set; }

        public Booking? Booking { get; set; }
        public Service? Service { get; set; }
    }
}
