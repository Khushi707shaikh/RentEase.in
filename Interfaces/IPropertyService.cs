using RentEase.DTOs;
using RentEase.Models;

namespace RentEase.Interfaces
{
    public interface IPropertyService
    {
        Task<List<Property>> GetPropertiesByLandlordAsync(int landlordId);

        Task<Property?> GetPropertyByIdAsync(int propertyId, int landlordId);

        Task AddPropertyAsync(Property property);

        Task<bool> UpdatePropertyAsync(Property property, int landlordId);

        Task<bool> DeletePropertyAsync(int propertyId, int landlordId);
        Task<DashboardDTO> GetDashboardSummaryAsync(int landlordId);
    }
}