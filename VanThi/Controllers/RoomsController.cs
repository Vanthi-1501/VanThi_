using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoomsController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            return room;
        }

        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, Room updatedRoom)
        {
            if (id != updatedRoom.Id)
                return BadRequest();

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            room.Code = updatedRoom.Code;
            room.Status = updatedRoom.Status;
            room.RoomTypeId = updatedRoom.RoomTypeId;

            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut) return BadRequest("Thời gian nhận phòng phải trước thời gian trả phòng.");

            // Tìm IDs các phòng đã có lịch đặt hoặc đang có khách trong khoảng thời gian yêu cầu
            var busyRoomIds = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed)
                .Where(b => b.CheckIn < checkOut && b.CheckOut > checkIn)
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            var availableRooms = await _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => !busyRoomIds.Contains(r.Id))
                .ToListAsync();

            return availableRooms;
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] RoomStatus status)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            try
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xoá thành công" });
            }
            catch (DbUpdateException)
            {
                return BadRequest("Không thể xóa phòng này vì đang có dữ liệu liên quan (lịch sử lưu trú hoặc hóa đơn).");
            }
        }
    }
}