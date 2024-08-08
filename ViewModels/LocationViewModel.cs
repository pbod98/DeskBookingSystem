using System.ComponentModel.DataAnnotations;

namespace DeskBookingSystem.ViewModels
{
    public class LocationViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Location name is required")]
        public string Name { get; set; }
    }
}
