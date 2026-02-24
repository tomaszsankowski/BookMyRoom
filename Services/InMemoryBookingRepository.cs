using System.Collections.Concurrent;
using BookMyRoom.Models;

namespace BookMyRoom.Services
{
    public interface IBookingRepository
    {
        IReadOnlyCollection<Room> GetRooms();
        bool AddRoom(Room room, out string? error);
        bool RemoveRoom(Guid id, out string? error);

        IReadOnlyCollection<Reservation> GetReservations();
        IReadOnlyCollection<Reservation> GetReservationsForDay(DateTime day);
        IReadOnlyCollection<Reservation> GetReservationsForUser(string login);
        bool CreateReservation(string userLogin, Guid roomId, DateTime start, DateTime end, out string? error, out Reservation? created);
        bool CancelReservation(Guid id, string userLogin, out string? error);

        void InitSampleData();
    }

    public class InMemoryBookingRepository : IBookingRepository
    {
        private readonly ConcurrentDictionary<Guid, Room> _rooms = new();
        private readonly ConcurrentDictionary<Guid, Reservation> _reservations = new();
        private readonly object _lock = new();

        public IReadOnlyCollection<Room> GetRooms() => _rooms.Values.OrderBy(r => r.Name).ToList();

        public bool AddRoom(Room room, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(room.Name)) { error = "Name required"; return false; }
            if (_rooms.Values.Any(r => string.Equals(r.Name, room.Name, StringComparison.OrdinalIgnoreCase)))
            { error = "Room with this name already exists"; return false; }
            room.Id = Guid.NewGuid();
            _rooms[room.Id] = room;
            return true;
        }

        public bool RemoveRoom(Guid id, out string? error)
        {
            error = null;
            if (_reservations.Values.Any(r => r.RoomId == id && r.End > DateTime.Now))
            { error = "Room has future reservations"; return false; }
            return _rooms.TryRemove(id, out _);
        }

        public IReadOnlyCollection<Reservation> GetReservations() => _reservations.Values.ToList();

        public IReadOnlyCollection<Reservation> GetReservationsForDay(DateTime day)
        {
            var start = day.Date;
            var end = start.AddDays(1);

            return _reservations.Values
                .Where(r => r.Start < end && r.End > start)
                .ToList();
        }

        public IReadOnlyCollection<Reservation> GetReservationsForUser(string login)
            => _reservations.Values.Where(r => string.Equals(r.UserLogin, login, StringComparison.OrdinalIgnoreCase) && r.End > DateTime.Now)
                                   .OrderBy(r => r.Start)
                                   .ToList();

        public bool CreateReservation(string userLogin, Guid roomId, DateTime start, DateTime end, out string? error, out Reservation? created)
        {
            error = null; created = null;
            if (!_rooms.ContainsKey(roomId)) { error = "Room not found"; return false; }
            if (start >= end) { error = "Start must be before end"; return false; }
            var duration = end - start;
            if (duration < TimeSpan.FromMinutes(15)) { error = "Minimum reservation is 15 minutes"; return false; }
            if (duration > TimeSpan.FromHours(3)) { error = "Maximum reservation is 3 hours"; return false; }

            lock (_lock)
            {
                bool overlaps = _reservations.Values.Any(r => r.RoomId == roomId && r.Start < end && r.End > start);
                if (overlaps) { error = "Timeslot already booked"; return false; }

                var res = new Reservation
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    UserLogin = userLogin,
                    Start = start,
                    End = end
                };
                _reservations[res.Id] = res;
                created = res;
                return true;
            }
        }

        public bool CancelReservation(Guid id, string userLogin, out string? error)
        {
            error = null;
            if (!_reservations.TryGetValue(id, out var res)) { error = "Reservation not found"; return false; }
            if (!string.Equals(res.UserLogin, userLogin, StringComparison.OrdinalIgnoreCase)) { error = "Not your reservation"; return false; }
            return _reservations.TryRemove(id, out _);
        }

        public void InitSampleData()
        {
            _rooms.Clear();
            _reservations.Clear();

            AddRoom(new Room { Name = "France", Capacity = 6 }, out _);
            AddRoom(new Room { Name = "Poland", Capacity = 12 }, out _);
            AddRoom(new Room { Name = "Spain", Capacity = 4 }, out _);

            var today = DateTime.Today;
            foreach (var (id, _) in _rooms)
            {
                CreateReservation("admin", id, today.AddHours(13), today.AddHours(14), out _, out _);
                CreateReservation("user", id, today.AddHours(15), today.AddHours(17), out _, out _);
            }
        }
    }
}
