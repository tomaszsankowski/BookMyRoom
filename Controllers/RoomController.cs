using BookMyRoom.Models;
using BookMyRoom.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookMyRoom.Controllers
{
    public class RoomController : Controller
    {
        private readonly IBookingRepository _repo;
        public RoomController(IBookingRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Manage()
        {
            if (!AccountController.IsAdmin(HttpContext)) return StatusCode(403);
            return View(_repo.GetRooms());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Room room)
        {
            if (!AccountController.IsAdmin(HttpContext)) return StatusCode(403);
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join("\n", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Manage");
            }
            if (!_repo.AddRoom(room, out var error)) TempData["Error"] = error;
            else TempData["Message"] = "Room added";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            if (!AccountController.IsAdmin(HttpContext)) return StatusCode(403);
            if (!_repo.RemoveRoom(id, out var error)) TempData["Error"] = error;
            else TempData["Message"] = "Room deleted";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Init()
        {
            if (!AccountController.IsAdmin(HttpContext)) return StatusCode(403);
            _repo.InitSampleData();
            TempData["Message"] = "Initialized";
            return RedirectToAction("Manage");
        }
    }
}
