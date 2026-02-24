using System.ComponentModel.DataAnnotations;

namespace BookMyRoom.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserLogin { get; set; } = string.Empty;

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }
    }
}
