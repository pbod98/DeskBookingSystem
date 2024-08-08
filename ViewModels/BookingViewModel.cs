namespace DeskBookingSystem.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string DeskName { get; set; }
        public int DeskId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
    }
}
