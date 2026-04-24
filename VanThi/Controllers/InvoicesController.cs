using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public InvoicesController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            return await _context.Invoices
                .Include(i => i.Stay)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.Customer)
                .Include(i => i.Stay)
                    .ThenInclude(s => s.Room)
                .OrderByDescending(i => i.Id)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Stay)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.Customer)
                .Include(i => i.Stay)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(i => i.Stay)
                    .ThenInclude(s => s.ServiceOrders)
                        .ThenInclude(so => so.Service)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return invoice;
        }
    }
}
