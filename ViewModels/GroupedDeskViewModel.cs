namespace DeskBookingSystem.ViewModels
{
    public class GroupedDeskViewModel
    {
        public string Location { get; set; }
        public List<DeskViewModel> Desks { get; set; }
    }
}
