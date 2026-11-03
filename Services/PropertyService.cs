using Microsoft.EntityFrameworkCore;
using RentEase.Data;
using RentEase.DTOs;
using RentEase.Interfaces;
using RentEase.Models;

namespace RentEase.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly RentEaseDbContext _context;

        public PropertyService(RentEaseDbContext context)
        {
            _context = context;
        }

        // Display properties belonging to a particular landlord
        public async Task<List<Property>> GetPropertiesByLandlordAsync(int landlordId)
        {
            return await _context.Properties
                .Where(p => p.LandlordId == landlordId)
                .ToListAsync();
        }

        // Get a property only if it belongs to the landlord
        public async Task<Property?> GetPropertyByIdAsync(int propertyId, int landlordId)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == propertyId &&
                    p.LandlordId == landlordId);
        }

        // Add property
        public async Task AddPropertyAsync(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
        }

        // Update property
        public async Task<bool> UpdatePropertyAsync(Property property, int landlordId)
        {
            var existingProperty = await GetPropertyByIdAsync(
                property.PropertyId, landlordId);

            if (existingProperty == null)
                return false;

            existingProperty.PropertyName = property.PropertyName;
            existingProperty.Address = property.Address;
            existingProperty.City = property.City;
            existingProperty.State = property.State;
            existingProperty.Description = property.Description;
            existingProperty.TotalRooms = property.TotalRooms;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete property
        public async Task<bool> DeletePropertyAsync(int propertyId, int landlordId)
        {
            var property = await _context.Properties
                .Include(p => p.Rooms)
                .FirstOrDefaultAsync(p =>
                    p.PropertyId == propertyId &&
                    p.LandlordId == landlordId);

            if (property == null || property.Rooms.Count > 0)
                return false;

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<DashboardDTO> GetDashboardSummaryAsync(int landlordId)
        {
            var properties = _context.Properties
                .Where(p => p.LandlordId == landlordId);

            var propertyIds = properties.Select(p => p.PropertyId);

            var rooms = _context.Rooms
                .Where(r => propertyIds.Contains(r.PropertyId));

            var totalProperties = await properties.CountAsync();

            var totalRooms = await rooms.CountAsync();

            var occupiedRooms = await rooms
                .CountAsync(r => r.Status == "Occupied");

            var availableRooms = await rooms
                .CountAsync(r => r.Status == "Available");

            var totalTenants = await _context.Tenants
                .CountAsync(t => t.LandlordId == landlordId);

            var totalRentCollected = await _context.RentPayments
                .Where(p => p.RentalAgreement.RoomAllocation.Room.Property.LandlordId == landlordId
                            && p.PaymentStatus == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var nextMonthStart = currentMonthStart.AddMonths(1);

            var currentMonthRentCollected = await _context.RentPayments
                .Where(p =>
                    p.RentalAgreement.RoomAllocation.Room.Property.LandlordId == landlordId
                    && p.PaymentStatus == "Paid"
                    && p.PaymentDate >= currentMonthStart
                    && p.PaymentDate < nextMonthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            return new DashboardDTO
            {
                TotalProperties = totalProperties,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                AvailableRooms = availableRooms,
                TotalTenants = totalTenants,
                TotalRentCollected = totalRentCollected,
                CurrentMonthRentCollected = currentMonthRentCollected
            };
        }
    }
}