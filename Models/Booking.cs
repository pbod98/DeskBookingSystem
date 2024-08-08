using System.ComponentModel.DataAnnotations.Schema;

namespace DeskBookingSystem.Models
{
    [Table("Bookings")]
    public class Booking
    {
        public int Id { get; set; }
        public int DeskId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public User User { get; set; }
        public Desk Desk { get; set; }

    }
}
