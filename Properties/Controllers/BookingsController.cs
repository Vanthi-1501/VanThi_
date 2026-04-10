using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public BookingsController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost("{id}/checkoout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            booking.CheckOut = DateTime.Now;

            var hours = (booking.CheckOut.Value - booking.CheckIn).TotalHours;
            if (hours < 1) hours = 1;

            var total = (decimal)hours * booking.Room.RoomType.Price;

            var services = await _context.ServiceOrders
                .Where(s => s.BookingId == id)
                .Include(s => s.Service)
                .ToListAsync();

            foreach (var s in services)
            {
                total += s.Quantity * s.Service.Price;
            }

            var payment = new Payment
            {
                BookingId = id,
                TotalAmount = total,
                PaymentDate = DateTime.Now
            };

            booking.Room.Status = "Đang dọn dẹp";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { total });
        }
    }
}