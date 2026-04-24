using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ServicesController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return service;
        }

        [HttpPost]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return Ok(service);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, Service updated)
        {
            if (id != updated.Id) return BadRequest();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            service.Name = updated.Name;
            service.Price = updated.Price;

            await _context.SaveChangesAsync();
            return Ok(service);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa dịch vụ thành công" });
        }
    }
}
