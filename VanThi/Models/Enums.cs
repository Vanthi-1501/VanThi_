namespace VanThi.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        Completed,
        Cancelled
    }

    public enum RoomStatus
    {
        Available,
        Reserved,
        Occupied,
        Cleaning
    }

    public enum InvoiceStatus
    {
        Unpaid,
        PartiallyPaid,
        Paid
    }
}
