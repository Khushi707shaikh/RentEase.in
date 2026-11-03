namespace RentEase.Interfaces
{
    public interface IEmailService
    {
        string GenerateWelcomeMessage(string tenantName);
    }
}