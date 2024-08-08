using DeskBookingSystem.Models;
using DeskBookingSystem.Services;
using DeskBookingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DeskBookingSystem.Controllers
{
    public class Dashboard : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BookingService _bookingService;
        public Dashboard(ApplicationDbContext context, BookingService service) 
        {
            _context = context;
            _bookingService = service;
        }
        public ActionResult Index(int userId)
        {
            var roleId = _context.Roles.Where(r => r.Name == "Administrator").FirstOrDefault().Id;
            var adminUser = _context.UserRoles.Where(r => r.UserId == userId && r.RoleId == roleId).FirstOrDefault();
            var viewModel = new DashboardViewModel
            {
                isAdmin = adminUser != null ? true : false,
                UserId = userId
            };

            var deskList = _context.Desks
                .Include(d => d.Location)
                .Include(d => d.Bookings).ThenInclude(b => b.User)
                .ToList();

            var groupedViewModel = deskList
                .GroupBy(d => d.Location.Name)
                .Select(g => new GroupedDeskViewModel
                {
                    Location = g.Key,
                    Desks = g.Select(d => new DeskViewModel
                    {
                        DeskId = d.Id,
                        DeskName = d.Name,
                        Description = d.Description,
                        Bookings = d.Bookings.Select(b => new BookingViewModel
                        {
                            UserName = b.User.UserName,
                            StartTime = b.StartTime.ToString("MM/dd/yyyy"),
                            EndTime = b.EndTime.ToString("MM/dd/yyyy")
                        }).ToList()
                    }).ToList()
                }).ToList();

            viewModel.Desks = groupedViewModel;

            return View(viewModel);
        }
       
        [Route("Dashboard/FilterByLocation")]
        [HttpPost]
        public IActionResult FilterByLocation(DashboardViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.LocationQuery))
            {
                ModelState.AddModelError(string.Empty, "Location name cannot be empty.");
                return RedirectToAction("Index", viewModel.UserId);
            }

            var location = _context.Locations.Where(l => l.Name == viewModel.LocationQuery).FirstOrDefault();

            if (location == null)
            {
                ModelState.AddModelError(string.Empty, "This location does not exist.");
                return RedirectToAction("Index", viewModel.UserId);
            }

            var deskList = _context.Desks
                .Include(d => d.Location)
                .Include(d => d.Bookings).ThenInclude(b => b.User)
                .Where(d => d.LocationId == location.Id)
                .ToList();

            var searchResult = deskList
                .Select(d => new DeskViewModel
                {
                    DeskId = d.Id,
                    DeskName = d.Name,
                    Description = d.Description,
                    Bookings = d.Bookings.Select(b => new BookingViewModel
                    {
                        UserName = b.User.UserName,
                        StartTime = b.StartTime.ToString("MM/dd/yyyy"),
                        EndTime = b.EndTime.ToString("MM/dd/yyyy")
                    }).ToList()
                }).ToList();

            viewModel.Desks = new List<GroupedDeskViewModel>
                {
                    new GroupedDeskViewModel
                    {
                        Desks = searchResult,
                        Location = location.Name
                    }
                };

            return View("Index", viewModel);
        }
        [HttpGet]
        public IActionResult BookDesk(int deskId, int userId)
        {
            var Desk = _context.Desks.Include(d => d.Bookings).FirstOrDefault();
            var viewModel = new BookingViewModel { DeskId  = deskId, UserId = userId };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult BookDesk(BookingViewModel viewModel)
        {
            if (viewModel.EndTime == null || viewModel.StartTime == null)
            {
                ModelState.AddModelError(string.Empty, "Some dates are missing.");
                return View(viewModel);
            }

            string pattern = @"^(0[1-9]|1[0-2])\/(0[1-9]|[1-2][0-9]|3[0-1])\/\d{4}$";
            Regex regex = new Regex(pattern);
            if(!regex.IsMatch(viewModel.StartTime) || !regex.IsMatch(viewModel.EndTime))
            {
                ModelState.AddModelError(string.Empty, "Invalid date");
                return View(viewModel);
            }

            var startTime = DateTime.ParseExact(viewModel.StartTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var endTime = DateTime.ParseExact(viewModel.EndTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var difference = startTime - endTime;
            if (Math.Abs(difference.TotalDays) > 5)
            {
                ModelState.AddModelError(string.Empty, "You can book the desk for max. 5 days.");
                return View(viewModel);
            }

            if(_bookingService.IsDeskBooked(viewModel.DeskId, startTime, endTime, viewModel.UserId))
            {
                ModelState.AddModelError(string.Empty, "The desk is not available");
                return View(viewModel);
            }
            if(!_bookingService.CanUserBook(viewModel.UserId, startTime))
            {
                ModelState.AddModelError(string.Empty, "You cannot book 24 hours or less before your previous reservation.");
                return View(viewModel);
            }

            var booking = new Booking
            {
                DeskId = viewModel.DeskId,
                StartTime = startTime,
                EndTime = endTime,
                UserId = viewModel.UserId
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
