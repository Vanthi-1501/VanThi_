using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoomTypesController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRoomTypes()
        {
            var roomTypes = await _context.RoomTypes.ToListAsync();
            var result = new List<object>();

            foreach (var type in roomTypes)
            {
                var rooms = await _context.Rooms.Where(r => r.RoomTypeId == type.Id).ToListAsync();
                var roomIds = rooms.Select(r => r.Id).ToList();

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Kiểm tra xem phòng nào đang bận (đã được duyệt hoặc đang chờ duyệt)
                var busyRoomIds = await _context.Bookings
                    .Where(b => roomIds.Contains(b.RoomId) && 
                                b.Status != BookingStatus.Cancelled &&
                                b.Status != BookingStatus.Completed &&
                                b.CheckIn < tomorrow && 
                                b.CheckOut > today)
                    .Select(b => b.RoomId)
                    .Distinct()
                    .ToListAsync();

                int availableCount = rooms.Count - busyRoomIds.Count;

                result.Add(new {
                    Id = type.Id,
                    Name = type.Name,
                    Price = type.Price,
                    AvailableCount = availableCount,
                    IsAvailable = availableCount > 0
                });
            }

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomType>> GetRoomType(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);

            if (roomType == null)
            {
                return NotFound();
            }

            return roomType;
        }

        [HttpPost]
        public async Task<ActionResult<RoomType>> CreateRoomType(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();
            return Ok(roomType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, RoomType updated)
        {
            if (id != updated.Id) return BadRequest();

            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return NotFound();

            roomType.Name = updated.Name;
            roomType.Price = updated.Price;

            await _context.SaveChangesAsync();
            return Ok(roomType);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return NotFound();

            // Kiểm tra xem có phòng nào thuộc loại này không
            var hasRooms = await _context.Rooms.AnyAsync(r => r.RoomTypeId == id);
            if (hasRooms) return BadRequest("Không thể xóa loại phòng này vì đang có phòng thuộc loại này.");

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa loại phòng thành công" });
        }
    }
}
