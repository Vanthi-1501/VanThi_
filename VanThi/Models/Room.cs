using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VanThi.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        public string Status { get; set; }

        public int RoomTypeId { get; set; }

        public RoomType RoomType { get; set; }
    }
}