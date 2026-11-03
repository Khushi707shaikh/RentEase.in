using RentEase.DTOs;
using RentEase.Models;

namespace RentEase.Interfaces
{
    public interface IRentalService
    {
        Task<List<RentalAgreement>> GetAgreementsByLandlordAsync(int landlordId);

        Task<List<RentPayment>> GetPaymentsForTenantAsync(int tenantUserId);

        Task<bool> AddRentPaymentAsync(RentPayment payment, int landlordId);
        Task<TenantDashboardDTO?> GetTenantDashboardAsync(int tenantUserId);
        // Get tenants created by a landlord
        Task<List<Tenant>> GetTenantsByLandlordAsync(int landlordId);

        // Get available rooms belonging to a landlord
        Task<List<Room>> GetAvailableRoomsByLandlordAsync(int landlordId);

        // Assign a tenant to a room
        Task<bool> AssignRoomAsync(AssignRoomDTO model, int landlordId);
        Task<List<TenantListDTO>> GetTenantListByLandlordAsync(int landlordId);
        Task<List<UtilityBill>> GetAgreementsForUtilityBillsAsync(int landlordId);

        Task<bool> AddUtilityBillAsync(UtilityBill bill, int landlordId);
        Task<List<UtilityBill>> GetUtilityBillsForTenantAsync(int tenantUserId);
    }
}