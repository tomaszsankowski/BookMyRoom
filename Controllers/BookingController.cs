using BookMyRoom.Models;
using BookMyRoom.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookMyRoom.Controllers
{
    [Route("Booking")]
    public class BookingController : Controller
    {
        private readonly IBookingRepository _repo;
        public BookingController(IBookingRepository repo)
        {
            _repo = repo;
        }

        private string? CurrentUser => AccountController.GetCurrentLogin(HttpContext);

        [HttpGet("Calendar")]
        public IActionResult Calendar()
        {
            if (CurrentUser is null) return RedirectToAction("Index", "Home");

            ViewBag.Rooms = _repo.GetRooms();
            return View();
        }

        [HttpGet("GetForDay")]
        public IActionResult GetForDay(DateTime day)
        {
            if (CurrentUser is null) return Unauthorized();

            var reservations = _repo.GetReservationsForDay(day)
                .Select(r => new { r.Id, r.RoomId, r.UserLogin, r.Start, r.End });
            return Json(reservations);
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] CreateDto dto)
        {
            if (CurrentUser is null) return Unauthorized();

            if (_repo.CreateReservation(CurrentUser, dto.RoomId, dto.Start, dto.End, out var error, out var created))
            {
                return Json(new { ok = true, reservation = created });
            }
            return Json(new { ok = false, error });
        }

        [HttpGet("MyBookings")]
        public IActionResult MyBookings()
        {
            if (CurrentUser is null) return RedirectToAction("Index", "Home");

            var list = _repo.GetReservationsForUser(CurrentUser);
            ViewBag.Rooms = _repo.GetRooms().ToDictionary(r => r.Id, r => r);
            return View(list);
        }

        [HttpPost("Cancel")]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(Guid id)
        {
            if (CurrentUser is null) return RedirectToAction("Index", "Home");

            if (_repo.CancelReservation(id, CurrentUser, out var error))
            {
                TempData["Message"] = "Reservation canceled";
            }
            else
            {
                TempData["Error"] = error;
            }
            return RedirectToAction("MyBookings");
        }

        [HttpGet("ExportMyBookings")]
        public IActionResult ExportMyBookings()
        {
            if (CurrentUser is null) return Unauthorized();

            var list = _repo.GetReservationsForUser(CurrentUser);
            var rooms = _repo.GetRooms().ToDictionary(r => r.Id, r => r);
            var ics = IcsBuilder.BuildICal(list, rooms, CurrentUser);
            var bytes = System.Text.Encoding.UTF8.GetBytes(ics);
            return File(bytes, "text/calendar", $"bookings_{CurrentUser}.ics");
        }
    }

    internal static class IcsBuilder
    {
        public static string BuildICal(IEnumerable<Reservation> reservations, IDictionary<Guid, Room> rooms, string user)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//lab2//Booking//EN");
            foreach (var r in reservations)
            {
                var room = rooms.TryGetValue(r.RoomId, out var rm) ? rm.Name : r.RoomId.ToString();
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{r.Id}@lab2");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
                sb.AppendLine($"DTSTART:{r.Start.ToUniversalTime():yyyyMMddTHHmmssZ}");
                sb.AppendLine($"DTEND:{r.End.ToUniversalTime():yyyyMMddTHHmmssZ}");
                sb.AppendLine($"SUMMARY:Rezerwacja - {room}");
                sb.AppendLine($"DESCRIPTION:Uzytkownik: {user}");
                sb.AppendLine("END:VEVENT");
            }
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }
    }
}
