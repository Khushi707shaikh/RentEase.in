using Microsoft.EntityFrameworkCore;
using RentEase.Data;
using RentEase.DTOs;
using RentEase.Interfaces;
using RentEase.Models;

namespace RentEase.Services
{
    public class RentalService : IRentalService
    {
        private readonly RentEaseDbContext _context;

        public RentalService(RentEaseDbContext context)
        {
            _context = context;
        }

        // Get all rental agreements belonging to a landlord
        public async Task<List<RentalAgreement>> GetAgreementsByLandlordAsync(int landlordId)
        {
            return await _context.RentalAgreements
                .Include(a => a.RoomAllocation)
                    .ThenInclude(a => a.Room)
                        .ThenInclude(r => r.Property)
                .Include(a => a.RoomAllocation)
                    .ThenInclude(a => a.Tenant)
                .Where(a => a.RoomAllocation.Room.Property.LandlordId == landlordId)
                .ToListAsync();
        }

        // Tenant can view only their own rent payment history
        public async Task<List<RentPayment>> GetPaymentsForTenantAsync(int tenantUserId)
        {
            return await _context.RentPayments
                .Include(p => p.RentalAgreement)
                    .ThenInclude(a => a.RoomAllocation)
                        .ThenInclude(a => a.Tenant)
                .Where(p => p.RentalAgreement.RoomAllocation.Tenant.UserId == tenantUserId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // Landlord records rent payment for their own property
        public async Task<bool> AddRentPaymentAsync(RentPayment payment, int landlordId)
        {
            bool agreementExists = await _context.RentalAgreements
                .AnyAsync(a =>
                    a.AgreementId == payment.AgreementId &&
                    a.RoomAllocation.Room.Property.LandlordId == landlordId);

            if (!agreementExists)
                return false;

            _context.RentPayments.Add(payment);
            await _context.SaveChangesAsync();

            return true;
        }
        // Get Tenant Dashboard Details
        public async Task<TenantDashboardDTO?> GetTenantDashboardAsync(int tenantUserId)
        {
            var tenant = await _context.Tenants
                .Include(t => t.User)
                .Include(t => t.RoomAllocations)
                    .ThenInclude(a => a.Room)
                        .ThenInclude(r => r.Property)
                .Include(t => t.RoomAllocations)
                    .ThenInclude(a => a.RentalAgreements)
                .FirstOrDefaultAsync(t => t.UserId == tenantUserId);

            if (tenant == null)
                return null;

            var allocation = tenant.RoomAllocations
                .Where(a => a.MoveOutDate == null)
                .OrderByDescending(a => a.MoveInDate)
                .FirstOrDefault();

            if (allocation == null)
            {
                return new TenantDashboardDTO
                {
                    TenantName = tenant.FullName,
                    Email = tenant.User.Email,
                    MobileNumber = tenant.MobileNumber
                };
            }

            var agreement = allocation.RentalAgreements
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefault();
            var totalRentPaid = await _context.RentPayments
    .Where(p =>
        p.RentalAgreement.RoomAllocation.Tenant.UserId == tenantUserId
        && p.PaymentStatus == "Paid")
    .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var pendingUtilityBills = await _context.UtilityBills
                .Where(b =>
                    b.RentalAgreement.RoomAllocation.Tenant.UserId == tenantUserId
                    && b.PaymentStatus == "Pending")
                .CountAsync();
            return new TenantDashboardDTO
            {
                TenantName = tenant.FullName,
                Email = tenant.User.Email,
                MobileNumber = tenant.MobileNumber,
                PropertyName = allocation.Room.Property.PropertyName,
                PropertyAddress = allocation.Room.Property.Address,
                RoomNumber = allocation.Room.RoomNumber,
                MonthlyRent = agreement?.MonthlyRent ?? allocation.Room.MonthlyRent,
                MoveInDate = allocation.MoveInDate,
                AgreementEndDate = agreement?.EndDate,
                TotalRentPaid = totalRentPaid,
                PendingUtilityBills = pendingUtilityBills
            };
        }
        // Get tenants belonging to a landlord
        public async Task<List<Tenant>> GetTenantsByLandlordAsync(int landlordId)
        {
            return await _context.Tenants
                .Where(t => t.LandlordId == landlordId &&
                            t.User.IsActive)
                .OrderBy(t => t.FullName)
                .ToListAsync();
        }
        // Get available rooms belonging to a landlord
        public async Task<List<Room>> GetAvailableRoomsByLandlordAsync(int landlordId)
        {
            return await _context.Rooms
                .Include(r => r.Property)
                .Where(r => r.Property.LandlordId == landlordId &&
                            r.Status == "Available")
                .OrderBy(r => r.Property.PropertyName)
                .ThenBy(r => r.RoomNumber)
                .ToListAsync();
        }
        // Assign tenant to room
        public async Task<bool> AssignRoomAsync(AssignRoomDTO model, int landlordId)
        {
            if (model.EndDate.Date <= model.MoveInDate.Date)
                return false;

            // Verify tenant belongs to this landlord
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t =>
                    t.TenantId == model.TenantId &&
                    t.LandlordId == landlordId &&
                    t.User.IsActive);

            if (tenant == null)
                return false;

            // Verify room belongs to this landlord
            var room = await _context.Rooms
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r =>
                    r.RoomId == model.RoomId &&
                    r.Property.LandlordId == landlordId &&
                    r.Status == "Available");

            if (room == null)
                return false;

            // Prevent assigning a tenant who already has an active room
            bool tenantAlreadyAssigned = await _context.RoomAllocations
                .AnyAsync(a =>
                    a.TenantId == model.TenantId &&
                    a.MoveOutDate == null);

            if (tenantAlreadyAssigned)
                return false;

            // Create room allocation
            var allocation = new RoomAllocation
            {
                TenantId = model.TenantId,
                RoomId = model.RoomId,
                MoveInDate = model.MoveInDate,
                MoveOutDate = null,
                Status = "Active"
            };

            // Create rental agreement
            var agreement = new RentalAgreement
            {
                RoomAllocation = allocation,
                StartDate = model.MoveInDate,
                EndDate = model.EndDate,
                MonthlyRent = model.MonthlyRent,
                SecurityDeposit = model.SecurityDeposit,
                AgreementStatus = "Active"
            };

            room.Status = "Occupied";

            _context.RoomAllocations.Add(allocation);
            _context.RentalAgreements.Add(agreement);

            await _context.SaveChangesAsync();

            return true;
        }
        // Get tenant list for a landlord
        public async Task<List<TenantListDTO>> GetTenantListByLandlordAsync(int landlordId)
        {
            var tenants = await _context.Tenants
                .Where(t => t.LandlordId == landlordId)
                .Include(t => t.User)
                .Include(t => t.RoomAllocations)
                    .ThenInclude(a => a.Room)
                        .ThenInclude(r => r.Property)
                .Include(t => t.RoomAllocations)
                    .ThenInclude(a => a.RentalAgreements)
                .ToListAsync();

            var tenantList = tenants.Select(t =>
            {
                var allocation = t.RoomAllocations
                    .Where(a => a.MoveOutDate == null)
                    .OrderByDescending(a => a.MoveInDate)
                    .FirstOrDefault();

                var agreement = allocation?.RentalAgreements
                    .OrderByDescending(a => a.StartDate)
                    .FirstOrDefault();

                return new TenantListDTO
                {
                    TenantId = t.TenantId,
                    TenantName = t.FullName,
                    Email = t.User.Email,
                    MobileNumber = t.MobileNumber,

                    PropertyName = allocation?.Room.Property.PropertyName
                                   ?? "Not Assigned",

                    RoomNumber = allocation?.Room.RoomNumber
                                 ?? "Not Assigned",

                    MonthlyRent = agreement?.MonthlyRent
                                  ?? allocation?.Room.MonthlyRent
                                  ?? 0,

                    AgreementStatus = agreement?.AgreementStatus
                                      ?? "Not Created"
                };
            }).ToList();

            return tenantList;
        }
        public async Task<List<UtilityBill>> GetAgreementsForUtilityBillsAsync(int landlordId)
        {
            return await _context.UtilityBills
                .Include(b => b.RentalAgreement)
                .ThenInclude(a => a.RoomAllocation)
                .ThenInclude(a => a.Room)
                .ThenInclude(r => r.Property)
                .Where(b => b.RentalAgreement.RoomAllocation.Room.Property.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task<bool> AddUtilityBillAsync(UtilityBill bill, int landlordId)
        {
            var agreement = await _context.RentalAgreements
                .Include(a => a.RoomAllocation)
                .ThenInclude(a => a.Room)
                .ThenInclude(r => r.Property)
                .FirstOrDefaultAsync(a =>
                    a.AgreementId == bill.AgreementId &&
                    a.RoomAllocation.Room.Property.LandlordId == landlordId);

            if (agreement == null)
                return false;

            if (bill.Amount <= 0 || bill.DueDate.Date < bill.BillingDate.Date)
                return false;

            bill.PaymentStatus = "Pending";

            _context.UtilityBills.Add(bill);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<List<UtilityBill>> GetUtilityBillsForTenantAsync(int tenantUserId)
        {
            return await _context.UtilityBills
                .Include(b => b.RentalAgreement)
                    .ThenInclude(a => a.RoomAllocation)
                        .ThenInclude(a => a.Tenant)
                .Where(b =>
                    b.RentalAgreement.RoomAllocation.Tenant.UserId == tenantUserId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

    }
}