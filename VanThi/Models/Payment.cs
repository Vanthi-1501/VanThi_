namespace VanThi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public Invoice? Invoice { get; set; }
    }
}