using Microsoft.AspNetCore.Mvc;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ServiceOrdersController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddService(ServiceOrder order)
        {
            var stay = await _context.Stays.FindAsync(order.StayId);
            if (stay == null || !stay.IsActive)
                return BadRequest("Không tìm thấy kỳ lưu trú đang hoạt động để gọi dịch vụ.");

            _context.ServiceOrders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}