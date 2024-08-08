namespace DeskBookingSystem.Services
{
    public class BookingService
    {
        private readonly ApplicationDbContext _context;
        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool IsDeskBooked(int deskId, DateTime startTime, DateTime endTime, int userId)
        {
            var existingBooking = _context.Bookings
                .Where(b => b.DeskId == deskId)
                .Any(b => (b.StartTime < endTime && b.EndTime > startTime));

            if (!existingBooking)
            {
                return false;
            }

            return true;
        }
        public bool CanUserBook(int userId, DateTime startTime)
        {
            var userHasConflictingReservation = _context.Bookings
                .Where(b => b.UserId == userId)
                .Any(b => (b.StartTime <= startTime.AddHours(24) && b.EndTime >= startTime));

            return !userHasConflictingReservation;
        }
    }
}
