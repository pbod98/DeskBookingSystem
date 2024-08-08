using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DeskBookingSystem.ViewModels
{
    public class UserViewModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }
        public string? Roles {  get; set; }
    }
}
