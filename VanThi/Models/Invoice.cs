using System;
using System.Collections.Generic;

namespace VanThi.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int StayId { get; set; }
        public decimal RoomCharges { get; set; }
        public decimal ServiceCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Stay? Stay { get; set; }
        public List<Payment>? Payments { get; set; }
    }
}
