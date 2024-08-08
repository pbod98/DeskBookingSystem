namespace DeskBookingSystem.ViewModels
{
    public class GroupedBookingViewModel
    {
        public string Desk { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
    }
}
