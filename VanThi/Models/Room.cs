using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VanThi.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        public int RoomTypeId { get; set; }
        
        public RoomType? RoomType { get; set; }
    }
}