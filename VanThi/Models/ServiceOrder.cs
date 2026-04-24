namespace VanThi.Models
{
    public class ServiceOrder
    {
        public int Id { get; set; }
        public int StayId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public Stay? Stay { get; set; }
        public Service? Service { get; set; }
    }
}