using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEase.Data;

namespace RentEase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly RentEaseDbContext _context;

        public ApiController(RentEaseDbContext context)
        {
            _context = context;
        }

        // GET: api/Api
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                success = true,
                message = "RentEase REST API is working!"
            });
        }

        // GET: api/Api/properties
        [HttpGet("properties")]
        public async Task<IActionResult> GetProperties()
        {
            var properties = await _context.Properties
                .Select(p => new
                {
                    p.PropertyId,
                    p.PropertyName,
                    p.Address,
                    p.City,
                    p.State,
                    p.TotalRooms,
                    p.LandlordId
                })
                .ToListAsync();

            return Ok(properties);
        }

        // GET: api/Api/tenants
        [HttpGet("tenants")]
        public async Task<IActionResult> GetTenants()
        {
            var tenants = await _context.Tenants
                .Select(t => new
                {
                    t.TenantId,
                    t.FullName,
                    t.MobileNumber,
                    t.Address,
                    t.LandlordId
                })
                .ToListAsync();

            return Ok(tenants);
        }

        // GET: api/Api/payments
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.RentPayments
                .Select(p => new
                {
                    p.PaymentId,
                    p.AgreementId,
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentMethod,
                    p.PaymentStatus
                })
                .ToListAsync();

            return Ok(payments);
        }
        // GET: api/Api/properties/1
        // Get single property details for editing
        [HttpGet("properties/{id:int}")]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await _context.Properties
                .Where(p => p.PropertyId == id)
                .Select(p => new
                {
                    p.PropertyId,
                    p.PropertyName,
                    p.Address,
                    p.City,
                    p.State,
                    p.Description,
                    p.TotalRooms,
                    p.LandlordId
                })
                .FirstOrDefaultAsync();

            if (property == null)
                return NotFound(new { message = "Property not found." });

            return Ok(property);
        }

        // PUT: api/Api/properties/1
        // Update property
        [HttpPut("properties/{id:int}")]
        public async Task<IActionResult> UpdateProperty(
            int id,
            [FromBody] PropertyUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
                return NotFound(new { message = "Property not found." });

            if (request.TotalRooms < 0)
                return BadRequest(new { message = "Total rooms cannot be negative." });

            var existingRooms = await _context.Rooms
                .CountAsync(r => r.PropertyId == id);

            if (request.TotalRooms < existingRooms)
                return BadRequest(new
                {
                    message = $"Total rooms cannot be less than existing rooms ({existingRooms})."
                });

            property.PropertyName = request.PropertyName;
            property.Address = request.Address;
            property.City = request.City;
            property.State = request.State;
            property.Description = request.Description;
            property.TotalRooms = request.TotalRooms;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Property updated successfully!",
                propertyId = property.PropertyId
            });
        }

        // DELETE: api/Api/properties/1
        // Delete property
        [HttpDelete("properties/{id:int}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
                return NotFound(new { message = "Property not found." });

            var hasRooms = await _context.Rooms
                .AnyAsync(r => r.PropertyId == id);

            if (hasRooms)
                return BadRequest(new
                {
                    message = "Cannot delete property because rooms are associated with it."
                });

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Property deleted successfully!"
            });
        }

        // Request model for property update
        public class PropertyUpdateRequest
        {
            public string PropertyName { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int TotalRooms { get; set; }
        }
    }
}