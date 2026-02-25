# BookMyRoom

A meeting room booking system built with **ASP.NET Core 8 MVC**. The application allows users to browse an interactive calendar, reserve conference rooms for specific time slots, manage their bookings, and export reservations to iCalendar (`.ics`) format. Administrators can manage the room inventory.

<img width="1920" height="947" alt="image" src="https://github.com/user-attachments/assets/b88c8423-034f-4ec3-b5d4-fc97856e363f" />

## Features

| Feature                  | Description                                                                                                           |
| :----------------------- | :-------------------------------------------------------------------------------------------------------------------- |
| **Interactive Calendar** | Daily schedule view with rooms as columns and hours as rows. Click an empty slot to book instantly.            |
| **Room Management**      | Admin panel for adding, listing, and deleting conference rooms (name + capacity).                                     |
| **Reservation System**   | Thread-safe booking with overlap detection, 15 min–3 h duration validation, and race-condition protection via `lock`. |
| **My Bookings**          | Personal view of upcoming reservations with one-click cancellation.                                                   |
| **iCalendar Export**     | Download all your upcoming bookings as an `.ics` file importable into Google Calendar / Outlook.                      |
| **Sample Data Seeding**  | Admin can seed predefined rooms and reservations with a single click.                                                 |
| **Session-Based Auth**   | Simple URL-based login (`/Account/Login/{login}`) with admin/user roles.                                              |

## Tech Stack

- **Framework:** ASP.NET Core 8 MVC (.NET 8)
- **Data Storage:** In-memory (`ConcurrentDictionary` + `lock` for atomic reservation logic)
- **Frontend:** Bootstrap 5, jQuery, Vanilla JS (AJAX)
- **DI:** Singleton `IBookingRepository` registered in the IoC container
- **Session:** Distributed memory cache with 30-minute idle timeout

## Project Structure

```
BookMyRoom/
├── Controllers/
│   ├── AccountController.cs      # Login / Logout
│   ├── BookingController.cs      # Calendar, Create, MyBookings, Cancel, Export
│   ├── ErrorController.cs        # 403 Access Denied
│   ├── HomeController.cs         # Landing page
│   └── RoomController.cs         # Admin room management + Init
├── Models/
│   ├── CreateDto.cs              # JSON DTO for new reservations
│   ├── Reservation.cs            # Reservation entity
│   └── Room.cs                   # Room entity
├── Services/
│   └── InMemoryBookingRepository.cs  # Thread-safe in-memory repository
├── Views/
│   ├── Booking/
│   │   ├── Calendar.cshtml       # Interactive booking calendar
│   │   └── MyBookings.cshtml     # User's reservations list
│   ├── Error/
│   │   └── AccessDenied.cshtml   # 403 page
│   ├── Home/
│   │   └── Index.cshtml          # Landing page
│   ├── Room/
│   │   └── Manage.cshtml         # Admin room CRUD
│   └── Shared/
│       └── _Layout.cshtml        # Master layout with Bootstrap navbar
├── wwwroot/                      # Static assets (CSS, JS, libraries)
├── Program.cs                    # App configuration & middleware pipeline
└── BookMyRoom.csproj             # Project file (.NET 8)
```

## Setup and Requirements

- **.NET SDK:** Version 8.0 or higher
- No external database required — all data is stored in memory.

## How to Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/<your-username>/BookMyRoom.git
   cd BookMyRoom
   ```
2. **Restore dependencies & run:**
   ```bash
   dotnet restore
   dotnet run
   ```
3. **Open in browser:**
   Navigate to `https://localhost:7068` (or the URL shown in the console).

4. **Quick login:**
   - Admin: click _"Zaloguj jako admin"_ in the navbar or go to `/Account/Login/admin`
   - User: click _"Zaloguj jako user"_ or go to `/Account/Login/user`

5. **Seed sample data (optional):**
   Log in as admin → Room Management → click _"Init"_ to create sample rooms and reservations.
