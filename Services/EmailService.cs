using RentEase.Interfaces;

namespace RentEase.Services
{
    public class EmailService : IEmailService
    {
        public string GenerateWelcomeMessage(string tenantName)
        {
            return $"Welcome to RentEase, {tenantName}! Your tenant account has been created successfully.";
        }
    }
}