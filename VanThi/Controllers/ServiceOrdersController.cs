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
            _context.ServiceOrders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}