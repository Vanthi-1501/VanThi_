using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace VanThi.Models
{
    [Table("RoomTypes")]
    public class RoomType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // ❌ tránh vòng lặp JSON
        [JsonIgnore]
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public decimal Price { get; set; }
        public string? Image { get; set; }
    }
}