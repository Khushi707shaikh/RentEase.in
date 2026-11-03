using RentEase.Interfaces;

namespace RentEase.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}