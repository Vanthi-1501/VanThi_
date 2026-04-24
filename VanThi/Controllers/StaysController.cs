using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaysController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public StaysController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stay>>> GetStays()
        {
            return await _context.Stays
                .Include(s => s.Booking)
                .Include(s => s.Room)
                .ToListAsync();
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stay = await _context.Stays
                    .Include(s => s.Booking)
                    .Include(s => s.Room)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (stay == null || !stay.IsActive) return NotFound("Không tìm thấy thông tin lưu trú đang hoạt động.");

                // 1. Cập nhật thông tin lưu trú
                stay.ActualCheckOut = DateTime.Now;
                stay.IsActive = false;

                // 2. Tính tiền dịch vụ
                var serviceOrders = await _context.ServiceOrders
                    .Where(so => so.StayId == stay.Id)
                    .Include(so => so.Service)
                    .ToListAsync();

                decimal serviceTotal = serviceOrders.Sum(so => so.Quantity * so.Service.Price);

                // 3. Cập nhật Invoice
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.StayId == stay.Id);
                if (invoice != null)
                {
                    invoice.ServiceCharges = serviceTotal;
                    invoice.TotalAmount = invoice.RoomCharges + serviceTotal;
                }

                // 4. Cập nhật trạng thái Booking và Phòng
                if (stay.Booking != null) stay.Booking.Status = BookingStatus.Completed;
                stay.Room.Status = RoomStatus.Cleaning;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Trả phòng thành công", totalAmount = invoice?.TotalAmount });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}
