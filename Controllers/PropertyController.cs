
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEase.Data;
using RentEase.DTOs;
using RentEase.Interfaces;
using RentEase.Models;
using System.Security.Claims;

namespace RentEase.Controllers
{
    [Authorize(Roles = "Landlord")]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly RentEaseDbContext _context;
        public PropertyController(
    IPropertyService propertyService,
    RentEaseDbContext context)
        {
            _propertyService = propertyService;
            _context = context;
        }

        // Get logged-in landlord ID
        private int GetLandlordId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );
        }


        // Display landlord properties with unit counts
        public async Task<IActionResult> Index()
        {
            int landlordId = GetLandlordId();

            var properties = await _context.Properties
                .Where(p => p.LandlordId == landlordId)
                .Select(p => new
                {
                    Property = p,
                    AddedUnits = p.Rooms.Count()
                })
                .ToListAsync();

            ViewBag.AddedUnits = properties.ToDictionary(
                x => x.Property.PropertyId,
                x => x.AddedUnits
            );

            return View(properties.Select(x => x.Property).ToList());
        }


        // GET: Add Property
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Add Property
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var property = new Property
            {
                PropertyName = model.PropertyName,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Description = model.Description,
                TotalRooms = model.TotalRooms,
                LandlordId = GetLandlordId()
            };

            await _propertyService.AddPropertyAsync(property);

            TempData["Success"] = "Property added successfully!";

            return RedirectToAction("Index");
        }

        // GET: Edit Property
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int landlordId = GetLandlordId();

            var property = await _propertyService
                .GetPropertyByIdAsync(id, landlordId);

            if (property == null)
                return NotFound();

            var model = new PropertyDTO
            {
                PropertyName = property.PropertyName,
                Address = property.Address,
                City = property.City,
                State = property.State,
                Description = property.Description,
                TotalRooms = property.TotalRooms
            };

            return View(model);
        }


        // POST: Edit Property
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyDTO model)
        {
            int landlordId = GetLandlordId();

            // Get property belonging to the logged-in landlord
            var existingProperty = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == id &&
                    p.LandlordId == landlordId);

            if (existingProperty == null)
                return NotFound();

            // Count existing rooms
            int currentRoomCount = await _context.Rooms
                .CountAsync(r => r.PropertyId == id);

            // Validate total units
            if (model.TotalRooms < currentRoomCount)
            {
                ModelState.AddModelError(
                    "TotalRooms",
                    $"You already have {currentRoomCount} units. Total units cannot be less than this."
                );
            }

            if (!ModelState.IsValid)
                return View(model);

            // Update property
            existingProperty.PropertyName = model.PropertyName;
            existingProperty.Address = model.Address;
            existingProperty.City = model.City;
            existingProperty.State = model.State;
            existingProperty.Description = model.Description;
            existingProperty.TotalRooms = model.TotalRooms;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Property updated successfully!";

            return RedirectToAction("Index");
        }

        // Delete Property
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _propertyService
                .DeletePropertyAsync(id, GetLandlordId());

            if (!deleted)
            {
                TempData["Error"] =
                    "Property could not be deleted. It may contain rooms.";
            }
            else
            {
                TempData["Success"] = "Property deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        // GET: Display rooms of a property
        public async Task<IActionResult> Rooms(int propertyId)
        {
            int landlordId = GetLandlordId();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == propertyId &&
                    p.LandlordId == landlordId);

            if (property == null)
                return NotFound();

            var rooms = await _context.Rooms
                .Where(r => r.PropertyId == propertyId)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            ViewBag.PropertyName = property.PropertyName;
            ViewBag.PropertyId = propertyId;

            return View(rooms);
        }

        // GET: Add Room
        public async Task<IActionResult> AddRoom(int propertyId)
        {
            int landlordId = GetLandlordId();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == propertyId &&
                    p.LandlordId == landlordId);

            if (property == null)
                return NotFound();

            return View(new RoomDTO { PropertyId = propertyId });
        }

        // POST: Add Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(RoomDTO model)
        {
            int landlordId = GetLandlordId();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == model.PropertyId &&
                    p.LandlordId == landlordId);

            if (property == null)
                return NotFound();

            bool exists = await _context.Rooms.AnyAsync(r =>
                r.PropertyId == model.PropertyId &&
                r.RoomNumber == model.RoomNumber);

            int currentRoomCount = await _context.Rooms
    .CountAsync(r => r.PropertyId == model.PropertyId);

            if (currentRoomCount >= property.TotalRooms)
            {
                ModelState.AddModelError("RoomNumber",
                    $"Maximum {property.TotalRooms} units are allowed for this property.");
            }

            if (exists)
                ModelState.AddModelError("RoomNumber", "This room number already exists.");

            if (!ModelState.IsValid)
                return View(model);

            var room = new Room
            {
                RoomNumber = model.RoomNumber,
                MonthlyRent = model.MonthlyRent,
                Capacity = model.Capacity,
                RoomType = model.RoomType,
                Status = model.Status,
                PropertyId = model.PropertyId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room added successfully.";

            return RedirectToAction("Rooms", new { propertyId = model.PropertyId });
        }

        // GET: Edit Room
        public async Task<IActionResult> EditRoom(int id)
        {
            int landlordId = GetLandlordId();

            var room = await _context.Rooms
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r =>
                    r.RoomId == id &&
                    r.Property.LandlordId == landlordId);

            if (room == null)
                return NotFound();

            var model = new RoomDTO
            {
                RoomNumber = room.RoomNumber,
                MonthlyRent = room.MonthlyRent,
                Capacity = room.Capacity,
                RoomType = room.RoomType,
                Status = room.Status,
                PropertyId = room.PropertyId
            };

            ViewBag.RoomId = room.RoomId;

            return View(model);
        }

        // POST: Edit Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, RoomDTO model)
        {
            int landlordId = GetLandlordId();

            var room = await _context.Rooms
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r =>
                    r.RoomId == id &&
                    r.Property.LandlordId == landlordId);

            if (room == null)
                return NotFound();

            bool duplicate = await _context.Rooms.AnyAsync(r =>
                r.PropertyId == room.PropertyId &&
                r.RoomNumber == model.RoomNumber &&
                r.RoomId != id);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "RoomNumber",
                    "This room number already exists."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoomId = id;
                return View(model);
            }

            room.RoomNumber = model.RoomNumber;
            room.MonthlyRent = model.MonthlyRent;
            room.Capacity = model.Capacity;
            room.RoomType = model.RoomType;
            room.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Room updated successfully!";

            return RedirectToAction("Rooms",
                new { propertyId = room.PropertyId });
        }

        // POST: Delete Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            int landlordId = GetLandlordId();

            var room = await _context.Rooms
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r =>
                    r.RoomId == id &&
                    r.Property.LandlordId == landlordId);

            if (room == null)
                return NotFound();

            // Check whether room has allocation history
            bool hasAllocations = await _context.RoomAllocations
                .AnyAsync(a => a.RoomId == id);

            if (room.Status == "Occupied" || hasAllocations)
            {
                TempData["Error"] =
                    "This room cannot be deleted because it is occupied or has rental allocation records.";

                return RedirectToAction("Rooms",
                    new { propertyId = room.PropertyId });
            }

            int propertyId = room.PropertyId;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room deleted successfully!";

            return RedirectToAction("Rooms",
                new { propertyId = propertyId });
        }
        // GET: Landlord Dashboard
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var summary = await _propertyService
                .GetDashboardSummaryAsync(landlordId);

            return View(summary);
        }
    }
}
