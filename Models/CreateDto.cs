namespace BookMyRoom.Models
{
    public record CreateDto(Guid RoomId, DateTime Start, DateTime End);
}
