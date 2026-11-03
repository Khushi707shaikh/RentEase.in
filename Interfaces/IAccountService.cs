using RentEase.DTOs;
using RentEase.Models;

namespace RentEase.Interfaces
{
    public interface IAccountService
    {
        Task<AppUser?> RegisterLandlordAsync(RegisterDTO model);

        Task<AppUser?> ValidateLoginAsync(LoginDTO model);
        Task<string?> CreateTenantAsync(TenantCreateDTO model, int landlordId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO model);
    }
}