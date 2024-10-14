using Dapper;
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
    public class OperatorController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IDbConnection _connection;
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDBContext _context;

        public OperatorController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
            IDbConnection connection, ILogger<CustomerController> logger, ApplicationDBContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            this.roleManager = roleManager;
            _connection = connection;
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Machine) // Include the related machine
                .ToListAsync();

            var bookingViewModels = bookings.Select(b => new BookingVM
            {
                Id = b.Id,
                UserId = b.UserId,
                MachineId = b.MachineId,
                MachineName = _context.Machines
                    .Where(m => m.Id == b.MachineId)
                    .Select(m => m.Name)
                    .FirstOrDefault(),
                Date = b.Date,
                Status = b.Status,
                IsCompleted = b.IsCompleted
            }).ToList();

            return View(bookingViewModels);
        }


        [HttpPost]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.IsAcknowledged = true; // Mark the booking as acknowledged
            await _context.SaveChangesAsync();

            TempData["Message"] = "Booking acknowledged successfully!";
            return RedirectToAction("Bookings"); // Redirect back to the bookings list
        }

        [HttpPost]
        public async Task<IActionResult> CompleteJob(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.IsCompleted = true; // Mark the booking as completed
            booking.Status = "Completed"; // Update the status
            await _context.SaveChangesAsync();

            TempData["Message"] = "Job completed successfully!";
            await NotifyCustomer(booking); // Notify customer
            return RedirectToAction("Bookings"); // Redirect back to the bookings list
        }

        private async Task NotifyCustomer(Booking booking)
        {
            // Logic to notify the customer (e.g., email, SMS) can be implemented here
            string message = $"Your booking for Machine ID {booking.MachineId} on {booking.Date.ToShortDateString()} has been completed.";
            // Use any notification service, e.g., email or SMS, to send this message
        }

        private string GetLoggedInUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
    }
}
