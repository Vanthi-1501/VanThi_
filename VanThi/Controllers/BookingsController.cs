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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .OrderByDescending(b => b.CheckIn)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            return booking;
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
        {
            if (booking.CheckIn >= booking.CheckOut) return BadRequest("Thời gian nhận phòng phải trước trả phòng.");

            // Logic chống Overbooking chính xác
            var isRoomBusy = await _context.Bookings
                .AnyAsync(b => b.RoomId == booking.RoomId && 
                               b.Status != BookingStatus.Cancelled &&
                               b.Status != BookingStatus.Completed &&
                               b.CheckIn < booking.CheckOut && 
                               b.CheckOut > booking.CheckIn);

            if (isRoomBusy) return BadRequest("Phòng này đã có lịch đặt giao thoa với thời gian bạn chọn.");

            var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.Id == booking.RoomId);
            if (room == null) return BadRequest("Phòng không tồn tại");

            // Tính toán giá ước tính
            var days = (booking.CheckOut - booking.CheckIn).TotalDays;
            if (days < 1) days = 1;
            booking.TotalPrice = (decimal)days * room.RoomType.Price;
            booking.Status = BookingStatus.Pending;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
                if (booking == null) return NotFound();

                if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
                    return BadRequest("Trạng thái đặt phòng không hợp lệ để nhận phòng.");

                // 1. Tạo Stay
                var stay = new Stay
                {
                    BookingId = booking.Id,
                    RoomId = booking.RoomId,
                    ActualCheckIn = DateTime.Now,
                    IsActive = true
                };

                // 2. Chuyển trạng thái Booking và Room
                booking.Status = BookingStatus.CheckedIn;
                booking.Room.Status = RoomStatus.Occupied;

                _context.Stays.Add(stay);
                await _context.SaveChangesAsync();

                // 3. Khởi tạo Invoice nháp
                var invoice = new Invoice
                {
                    StayId = stay.Id,
                    RoomCharges = booking.TotalPrice,
                    TotalAmount = booking.TotalPrice, // Gốc tiền phòng, service sẽ cộng sau
                    Status = InvoiceStatus.Unpaid
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Nhận phòng thành công", stayId = stay.Id, invoiceId = invoice.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
                return BadRequest("Chỉ có thể duyệt đơn ở trạng thái Chờ duyệt (Pending).");

            booking.Status = BookingStatus.Confirmed;
            // Chuyển trạng thái phòng sang Reserved để đánh dấu đã có chủ
            if (booking.Room != null) booking.Room.Status = RoomStatus.Reserved;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã chấp nhận đơn đặt phòng." });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            booking.Status = BookingStatus.Cancelled;
            // Nếu không còn ai đặt phòng này nữa, có thể trả về Available (logic overlap sẽ tự tính)
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối đơn đặt phòng." });
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm Stay đang hoạt động của Booking này
                var stay = await _context.Stays
                    .Include(s => s.Booking)
                    .Include(s => s.Room)
                    .FirstOrDefaultAsync(s => s.BookingId == id && s.IsActive);

                if (stay == null) return NotFound("Không tìm thấy thông tin lưu trú đang hoạt động cho đơn đặt phòng này.");

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
                    // Tạm thời để status là Unpaid hoặc chuyển sang Paid tùy quy trình, 
                    // nhưng CheckOut thành công thì booking hoàn tất.
                }

                // 4. Cập nhật trạng thái Booking và Phòng
                if (stay.Booking != null) stay.Booking.Status = BookingStatus.Completed;
                if (stay.Room != null) stay.Room.Status = RoomStatus.Cleaning;

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