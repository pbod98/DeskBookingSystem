using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeskBookingSystem.Models
{
    [Table("Desks")]
    public class Desk
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public Location Location { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
