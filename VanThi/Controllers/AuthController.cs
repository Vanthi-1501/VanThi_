using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public AuthController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Tên đăng nhập đã tồn tại");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác");

            // Vì bài toán đơn giản, tôi trả về thông tin user làm token giả
            // Trong thực tế sẽ dùng JWT
            return Ok(new
            {
                token = "fake-jwt-token-" + Guid.NewGuid().ToString(),
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Role,
                    user.Email
                }
            });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Người dùng không tồn tại");

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId || c.Email == user.Email);

            var bookings = new List<object>();
            var paidInvoices = new List<object>();

            if (customer != null)
            {
                bookings = await _context.Bookings
                    .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                    .Where(b => b.CustomerId == customer.Id)
                    .OrderByDescending(b => b.CheckIn)
                    .Select(b => new
                    {
                        b.Id,
                        RoomCode = b.Room.Code,
                        RoomTypeName = b.Room.RoomType.Name,
                        b.CheckIn,
                        b.CheckOut,
                        b.Status,
                        b.TotalPrice
                    })
                    .Cast<object>()
                    .ToListAsync();

                paidInvoices = await _context.Invoices
                    .Include(i => i.Stay)
                        .ThenInclude(s => s.Room)
                    .Where(i => i.Stay.Booking.CustomerId == customer.Id && i.Status == InvoiceStatus.Paid)
                    .Select(i => new
                    {
                        i.Id,
                        i.TotalAmount,
                        PaymentDate = DateTime.Now,
                        RoomName = i.Stay.Room.Code,
                        i.Status
                    })
                    .Cast<object>()
                    .ToListAsync();
            }

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Role
                },
                stats = new
                {
                    BookingCount = bookings.Count,
                    PaidInvoicesCount = paidInvoices.Count
                },
                bookings,
                paidInvoices
            });
        }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
