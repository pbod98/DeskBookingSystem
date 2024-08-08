namespace DeskBookingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public bool isAdmin {  get; set; }
        public int UserId { get; set; }
        public List<GroupedDeskViewModel> Desks { get; set; }
        public string LocationQuery { get; set; }
    }
}
