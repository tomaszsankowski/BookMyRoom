using System.ComponentModel.DataAnnotations;

namespace BookMyRoom.Models
{
    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(64, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 500)]
        public int Capacity { get; set; }
    }
}
