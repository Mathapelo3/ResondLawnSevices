﻿using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResondLawnSevices.Data;
using ResondLawnSevices.Models;
using ResondLawnSevices.ViewModel;
using System.Data;
using System.Security.Claims;

namespace ResondLawnSevices.Controllers
{
    public class CustomerController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IDbConnection _connection;
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDBContext _context;

        public CustomerController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, 
            IDbConnection connection, ILogger<CustomerController> logger, ApplicationDBContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            this.roleManager = roleManager;
            _connection = connection;
            _logger = logger;
            _context = context;
        }

        public IActionResult CustomerDashboard()
        {
            return View();
        }

        public async Task<ActionResult> Index()
        {
           
            return View();
        }

        // GET: RequestMachine
        [HttpGet]
        public IActionResult RequestMachine()
        {
            var machines = _context.Machines.ToList();

            // If no machines are available, set a flag
            ViewBag.NoMachinesAvailable = !machines.Any();

            // Map domain models to view models
            var machineVms = machines.Select(machine => new MachineVM
            {
                Id = machine.Id,
                Name = machine.Name,
                // Map other properties here if needed
            }).ToList();

            return View(machineVms);
        }

        // POST: RequestMachine
        [HttpPost]
        public async Task<IActionResult> RequestMachine(MachineBookingVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetLoggedInUserId();
            var date = model.Date;

            // Check if the machine is already booked for the requested date
            int bookingCount = await _context.Bookings
                .CountAsync(b => b.MachineId == model.MachineId && b.Date.Date == date.Date);

            if (bookingCount == 0)
            {
                // Machine is available, proceed with booking
                var booking = new Booking
                {
                    UserId = userId,
                    MachineId = model.MachineId,
                    Date = date,
                    Status = "Pending"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Redirect to confirmation
                return RedirectToAction("Confirmation", new { message = "Booking successful." });
            }
            else
            {
                // Machine is already booked, handle the conflict

                // Add conflict to the Conflicts table
                var conflict = new Conflicts
                {
                    UserId = userId,
                    MachineId = model.MachineId,
                    RequestedDate = date,
                    Status = "Routed to conflict manager"
                };

                _context.Conflicts.Add(conflict);
                await _context.SaveChangesAsync();

                // Check for alternative machines that are not booked on the same date
                var alternativeMachines = await _context.Machines
                    .Where(m => !_context.Bookings
                        .Any(b => b.MachineId == m.Id && b.Date.Date == date.Date)) // Machines not booked on that date
                    .Select(m => m.Id)
                    .ToListAsync();

                string message = alternativeMachines.Any()
                    ? $"Conflict: The selected machine is already booked. Suggested alternatives: {string.Join(", ", alternativeMachines)}."
                    : "Conflict: The selected machine is already booked and no alternative machines are available.";

                // Redirect to confirmation with conflict message
                return RedirectToAction("Confirmation", new { message });
            }
        }

        private string GetLoggedInUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        private bool IsMachineAvailable(int machineId, DateTime date)
        {
            var bookingCount = _context.Bookings
                .Count(b => b.MachineId == machineId && b.Date.Date == date.Date);

            return bookingCount == 0; // Return true if no bookings found
        }

        public async Task<IActionResult> CustomerBookings()
        {
            // Get the currently logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // Retrieve the bookings where the UserId matches the logged-in user
            var bookings = await _context.Bookings
                .Include(b => b.Machine) // Include related machine details
                .Include(b => b.User) // Include related user details (AppUser)
                .Where(b => b.UserId == userId) // Filter bookings for the current user
                .ToListAsync();

            // Map Booking entities to BookingVM
            var bookingVMs = bookings.Select(b => new BookingVM
            {
                Id = b.Id,
                UserId = b.UserId,
                CustomerName = b.User.Name,
                CustomerEmail = b.User.Email,
                CustomerAddress = b.User.Address,
                MachineId = b.MachineId,
                MachineName = b.Machine.Name,
                Date = b.Date,
                Status = b.Status,
                IsAcknowledged = b.IsAcknowledged,
                IsCompleted = b.IsCompleted
            }).ToList();

            // Return the view with the current user's booking data
            return View(bookingVMs);
        }


        public IActionResult Confirmation(string message)
        {
            ViewBag.Message = message;
            return View();
        }



    }
}
