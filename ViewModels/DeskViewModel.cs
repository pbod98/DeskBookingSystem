using System.ComponentModel.DataAnnotations;

namespace DeskBookingSystem.ViewModels
{
    public class DeskViewModel
    {
        public int DeskId { get; set; }
        public string DeskName { get; set; }
        public string? Description { get; set; }
        public string LocationName { get; set; }
        public int LocationId { get; set; }
        public bool IsAvailable { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
    }
}
