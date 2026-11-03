
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentEase.Data;
using RentEase.DTOs;
using RentEase.Interfaces;
using RentEase.Models;
using System.Security.Cryptography;

namespace RentEase.Services
{
    public class AccountService : IAccountService
    {
        private readonly RentEaseDbContext _context;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AccountService(RentEaseDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        // Register Landlord
        public async Task<AppUser?> RegisterLandlordAsync(RegisterDTO model)
        {
            bool emailExists = await _context.AppUsers
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
                return null;

            var user = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = "Landlord",
                MustChangePassword = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, model.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // Validate Login
        public async Task<AppUser?> ValidateLoginAsync(LoginDTO model)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.Email == model.Email && u.IsActive);

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        // Create Tenant Account
        public async Task<string?> CreateTenantAsync(
            TenantCreateDTO model, int landlordId)
        {
            // Check duplicate email
            bool emailExists = await _context.AppUsers
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
                return null;

            // Verify landlord
            bool validLandlord = await _context.AppUsers
                .AnyAsync(u => u.UserId == landlordId
                    && u.Role == "Landlord"
                    && u.IsActive);

            if (!validLandlord)
                return null;

            // Generate temporary password
            string temporaryPassword = GenerateTemporaryPassword();

            // Create AppUser
            var user = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = "Tenant",
                MustChangePassword = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(
                user, temporaryPassword);

            // Create Tenant record
            var tenant = new Tenant
            {
                FullName = model.FullName,
                MobileNumber = model.MobileNumber,
                Address = model.Address,
                CreatedAt = DateTime.Now,
                User = user,
                LandlordId = landlordId
            };

            _context.AppUsers.Add(user);
            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync();

            // Return temporary password to landlord
            return temporaryPassword;
        }

        // Generate secure random password
        private string GenerateTemporaryPassword()
        {
            const string characters =
                "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

            return new string(
                Enumerable.Range(0, 12)
                    .Select(_ => characters[
                        RandomNumberGenerator.GetInt32(characters.Length)])
                    .ToArray()
            );
        }
        // Change Tenant Password
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO model)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.Role == "Tenant" &&
                    u.IsActive);

            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(
                user, model.NewPassword);

            user.MustChangePassword = false;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
