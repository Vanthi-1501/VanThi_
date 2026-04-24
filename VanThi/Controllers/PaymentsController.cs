using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanThi.Data;
using VanThi.Models;

namespace VanThi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public PaymentsController(HotelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MakePayment([FromQuery] int invoiceId, [FromQuery] decimal amount, [FromQuery] string method = "Cash")
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound("Không tìm thấy hóa đơn.");

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                PaymentMethod = method,
                PaymentDate = DateTime.Now
            };

            invoice.PaidAmount += amount;

            if (invoice.PaidAmount >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Thanh toán thành công", 
                remaining = invoice.TotalAmount - invoice.PaidAmount,
                status = invoice.Status.ToString()
            });
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByInvoice(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .ToListAsync();
        }
    }
}
