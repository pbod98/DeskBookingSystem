using DeskBookingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DeskBookingSystem.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using DeskBookingSystem.Migrations;
using System.Data;

namespace DeskBookingSystem.Controllers
{
    public class AdminPanel : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminPanel(ApplicationDbContext context)
        {
            _context = context;
        }
        public ActionResult Manage()
        {
            return View();
        }
        public ActionResult Locations()
        {
            var locationList = _context.Locations.ToList();
            var listViewModel = new List<LocationViewModel>();

            foreach(var location in locationList)
            {
                listViewModel.Add(new LocationViewModel { Id = location.Id, Name = location.Name });
            }

            return View(listViewModel);
        }
        [HttpGet]
        public ActionResult AddLocation()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddLocation(LocationViewModel viewModel)
        {
            var existingLocation = _context.Locations.Where(l => l.Name == viewModel.Name).FirstOrDefault();

            if (existingLocation != null)
            {
                ModelState.AddModelError(string.Empty, "This location already exists.");
            }
            else
            {
                var Location = new Location { Name = viewModel.Name };
                _context.Locations.Add(Location);
                _context.SaveChanges();
            }

            return RedirectToAction("Locations");
        }
        [HttpGet]
        public ActionResult EditLocation(int locationId)
        {
            var location = _context.Locations.Find(locationId);

            if (location == null)
            {
                return NotFound();
            }

            var viewModel = new LocationViewModel { Name = location.Name, Id = locationId };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditLocation(LocationViewModel viewModel)
        {
            var Location = _context.Locations.Find(viewModel.Id);

            if (Location == null)
            {
                return NotFound();
            }

            var locationList = _context.Locations.ToList();
            foreach(var location in locationList)
            {
                if(location.Name == viewModel.Name && location.Id != viewModel.Id) 
                {
                    ModelState.AddModelError(string.Empty,"Location with this name already exists.");
                    return View(viewModel);
                }
            }

            Location.Name = viewModel.Name;
            _context.SaveChanges();

            return RedirectToAction("Locations");
        }
        [HttpGet]
        public ActionResult RemoveLocation(int locationId)
        {
            var location = _context.Locations.Include(l => l.Desks).Where(l => l.Id == locationId).FirstOrDefault();

            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }
        [HttpPost]
        public ActionResult RemoveLocation(Location location)
        {
            if (location == null)
            {
                return NotFound();
            }
            if(!location.Desks.IsNullOrEmpty())
            {
                ModelState.AddModelError(string.Empty, "This location cannot be removed because it contains some desks.");
                return View(location);
            }

            _context.Locations.Remove(location);
            _context.SaveChanges();

            return RedirectToAction("Locations");
        }
        public ActionResult Desks()
        {
            var deskList = _context.Desks.Include(d => d.Location).ToList();
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
                            LocationId = d.LocationId,
                            LocationName = d.Location.Name
                        }).ToList()
                    }).ToList();

            return View(groupedViewModel);
        }
        [HttpGet]
        public ActionResult AddDesk(int locationId)
        {
            var deskViewModel = new DeskViewModel
            {
                LocationName = _context.Locations.Find(locationId).Name,
                LocationId = locationId
            };

            return View(deskViewModel);
        }
        [HttpPost]
        public ActionResult AddDesk(DeskViewModel viewModel)
        {
            var location = _context.Locations.Include(l => l.Desks).Where(l => l.Id == viewModel.LocationId).FirstOrDefault();

            if(!location.Desks.IsNullOrEmpty())
            {
                foreach (var desk in location.Desks)
                {
                    if (desk.Name == viewModel.DeskName)
                    {
                        ModelState.AddModelError(string.Empty, "A desk with this name already exists in this location.");
                        return View(viewModel);
                    }
                }
            }
            if(viewModel.DeskName == null)
            {
                ModelState.AddModelError(string.Empty, "");
                return View(viewModel);
            }

            var Desk = new Desk
            {
                LocationId = viewModel.LocationId,
                Name = viewModel.DeskName,
                Description = viewModel.Description
            };
             _context.Desks.Add(Desk);
            _context.SaveChanges();

            return RedirectToAction("Desks");
        }
        [HttpGet]
        public ActionResult EditDesk(int deskId)
        {
            var desk = _context.Desks.Include(d => d.Location).Where(d => d.Id == deskId).FirstOrDefault();

            if (desk == null)
            {
                return NotFound();
            }

            var viewModel = new DeskViewModel
            {
                DeskName = desk.Name,
                LocationName = desk.Location.Name,
                DeskId = deskId,
                LocationId = desk.LocationId,
                Description = desk.Description
            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditDesk(DeskViewModel viewModel)
        {
            var Desk = _context.Desks.Include(d => d.Location).Where(d => d.Id == viewModel.DeskId).FirstOrDefault();

            if (Desk == null)
            {
                return NotFound();
            }

            var desksInLocation = Desk.Location.Desks;
            if (!desksInLocation.IsNullOrEmpty())
            {
                foreach(var desk in desksInLocation)
                {
                    if(desk.Name == viewModel.DeskName)
                    {
                        ModelState.AddModelError(string.Empty, "There already exists a desk with this name in this location.");
                        return View(viewModel);
                    }
                }
            }

            Desk.Name = viewModel.DeskName;
            Desk.Description = viewModel.Description;
            _context.SaveChanges();

            return RedirectToAction("Locations");
        }
        [HttpGet]
        public ActionResult RemoveDesk(int deskId)
        {
            var Desk = _context.Desks.Include(d => d.Bookings).Where(d => d.Id == deskId).FirstOrDefault();

            if (Desk == null)
            {
                return NotFound();
            }

            return View(Desk);
        }
        [HttpPost]
        public ActionResult RemoveDesk(DeskViewModel viewModel)
        {
            var Desk = _context.Desks.Find(viewModel.DeskId);

            if (Desk == null)
            {
                return NotFound();
            }
            if (!Desk.Bookings.IsNullOrEmpty())
            {
                foreach(var booking in Desk.Bookings)
                {
                    if(booking.Active)
                    {
                        ModelState.AddModelError(string.Empty, "This desk is already booked.");
                        return View(viewModel);
                    }
                }
            }

            _context.Desks.Remove(Desk);
            _context.SaveChanges();

            return RedirectToAction("Desks");
        }
        public ActionResult Users()
        {
            var userList = _context.Users.Include(u => u.UserRoles).ToList();
            var listViewModel = new List<UserViewModel>();

            foreach (var user in userList)
            {
                foreach(var userRole in user.UserRoles)
                {
                    var role = _context.Roles.Find(userRole.RoleId);
                    var roleString = string.Join(",", role.Name);
                    listViewModel.Add(new UserViewModel { UserName = user.UserName, Roles = roleString, Id = user.Id });
                }
            }

            return View(listViewModel);
        }

        [HttpGet]
        public ActionResult EditUserRole(int userId)
        {
            var user = _context.Users.Find(userId);
            var viewModel = new UserViewModel
            {
                UserName = user.UserName,
                Id = user.Id
            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditUserRole(UserViewModel viewModel)
        {
            var user = _context.Users.Include(u => u.UserRoles).Where(u => u.Id == viewModel.Id).FirstOrDefault();
            var adminRole = _context.Roles.Where(r => r.Name == "Administrator").FirstOrDefault();

            if (user.UserRoles.Any(r => r.RoleId == adminRole.Id))
            {
                ModelState.AddModelError(string.Empty, "This user already has admin access.");
                return View(viewModel);
            }

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            });

            return RedirectToAction("Users");
        }
        [HttpGet]
        public ActionResult RemoveUser(int userId)
        {
            var User = _context.Users.Find(userId);

            if (User == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel
            {
                Id = userId,
                UserName = User.UserName
            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult RemoveUser(UserViewModel viewModel)
        {
            var user = new User { Id = (int)viewModel.Id };
            _context.Entry(user).State = EntityState.Deleted;
            _context.SaveChanges();

            return RedirectToAction("Users");
        }
    }
}
