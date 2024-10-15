using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResondLawnSevices.Data;
using ResondLawnSevices.Models;
using ResondLawnSevices.Services;
using ResondLawnSevices.ViewModel;
using System.Security.Claims;

namespace ResondLawnSevices.Controllers
{
    public class ConflictManagerController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailServices _emailService;

        public ConflictManagerController(ApplicationDBContext context, IWebHostEnvironment webHostEnvironment, EmailServices emailService)
        {
            _context = context;
            _environment = webHostEnvironment;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var conflicts = await _context.Conflicts
                .Include(c => c.User)
                .ToListAsync();

            var machineIds = conflicts.Select(c => c.MachineId).Distinct().ToList();
            var machines = await _context.Machines
                .Where(m => machineIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name);

            var conflictViewModels = conflicts.Select(c => new ConflictVM
            {
                Id = c.Id,
                UserId = c.UserId,
                CustomerName = c.User.Name,
                CustomerEmail = c.User.Email,
                CustomerAddress = c.User.Address,
                MachineId = c.MachineId,
                MachineName = machines.TryGetValue(c.MachineId, out var machineName) ? machineName : "Unknown",
                RequestedDate = c.RequestedDate,
                Status = c.Status,
                AlternativeMachineName = "PowerCutter 112"
            });

            return View(conflictViewModels.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> ResolveConflict(int id)
        {
            var conflict = await _context.Conflicts
                .Include(c => c.User)
                .Include(c => c.Machine)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conflict == null)
            {
                return NotFound();
            }

            // Fetch all machines from the database
            var machines = await _context.Machines.ToListAsync();

            // Prepare ViewModel
            var conflictVM = new ConflictVM
            {
                Id = conflict.Id,
                UserId = conflict.UserId,
                CustomerName = conflict.User.Name,
                MachineId = conflict.MachineId,
                MachineName = conflict.Machine.Name,
                RequestedDate = conflict.RequestedDate,
                Status = conflict.Status,
            };

            // Pass the list of machines using ViewBag or add them to the ViewModel
            ViewBag.Machines = machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return View(conflictVM); // Render the resolve conflict view
        }

        [HttpPost]
        public async Task<IActionResult> ResolveConflict(int conflictId, int machineId)
        {
            // Find the conflict entity
            var conflict = await _context.Conflicts
                .Include(c => c.User)
                .Include(c => c.Machine)
                .FirstOrDefaultAsync(c => c.Id == conflictId);

            if (conflict == null)
            {
                return NotFound();
            }

            // Check if the machine exists
            var machineExists = await _context.Machines.AnyAsync(m => m.Id == machineId);
            if (!machineExists)
            {
                ViewBag.Message = "The selected machine does not exist. Please select a valid machine.";

                // Populate ViewModel and available machines for re-selection
                return await ReturnConflictViewModelWithMachines(conflict);
            }

            // Check if the machine is available for the requested date
            var isMachineAvailable = await _context.Bookings
                .AnyAsync(b => b.MachineId == machineId && b.Date.Date == conflict.RequestedDate.Date);

            if (isMachineAvailable)
            {
                ViewBag.Message = "The selected alternative machine is not available on this date. Please select another machine.";

                // Populate ViewModel and available machines for re-selection
                return await ReturnConflictViewModelWithMachines(conflict);
            }

            // Create a new booking for the selected machine
            var newBooking = new Booking
            {
                UserId = conflict.UserId,
                MachineId = machineId,
                Date = conflict.RequestedDate,
                Status = "Pending"
            };

            // Update conflict status and mark it as resolved
            conflict.Status = $"Resolved - Assigned to Machine ID {machineId}";

            // Save the new booking and update conflict
            _context.Bookings.Add(newBooking);
            _context.Conflicts.Update(conflict);
            await _context.SaveChangesAsync();

            try
            {
                // Notify the operator about the new booking, but don't stop the conflict resolution if this fails
                await NotifyOperator(newBooking);
            }
            catch (Exception ex)
            {
                // Log the exception or notify an admin, but allow conflict resolution to proceed
                Console.WriteLine($"Email notification failed: {ex.Message}");
            }

            // Redirect to the conflict manager's view (or an appropriate page)
            return RedirectToAction("Index");
        }

        // Helper method to return ViewModel with available machines
        private async Task<IActionResult> ReturnConflictViewModelWithMachines(Conflicts conflict)
        {
            // Create a ViewModel to pass to the view
            var conflictVM = new ConflictVM
            {
                Id = conflict.Id,
                UserId = conflict.UserId,
                CustomerName = conflict.User.Name,
                MachineId = conflict.MachineId,
                MachineName = conflict.Machine.Name,
                RequestedDate = conflict.RequestedDate,
                Status = conflict.Status
            };

            // Pass machines list again to the view
            var machines = await _context.Machines.ToListAsync();
            ViewBag.Machines = machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return View(conflictVM);
        }



        [HttpPost]
        public async Task<IActionResult> DeleteConflict(int conflictId)
        {
            var conflict = await _context.Conflicts.FindAsync(conflictId);
            if (conflict == null)
            {
                return NotFound();
            }

            _context.Conflicts.Remove(conflict);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        //public IActionResult Create()
        //{
            

        //    return View();
        //}


        //[HttpPost]
        //public async Task<IActionResult> Create(MachineVM vm)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(vm);
        //    }

        //    string stringFileName = UploadFile(vm);
        //    var machine = new Machine
        //    {
        //        Name = vm.Name,
        //        Description = vm.Description,
        //        ImageUrl = stringFileName,
        //        Status = vm.Status
        //    };

        //    await _context.Machines.AddAsync(machine);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Index");
        //}

        private string UploadFile(MachineVM vm)
        {
            string fileName = null;
            if (vm.Image != null)
            {
                string uploadDir = Path.Combine(_environment.WebRootPath, "Img");
                fileName = Guid.NewGuid() + "-" + vm.Image.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    vm.Image.CopyTo(fileStream);
                }
            }
            return fileName;
        }

        private async Task NotifyOperator(Booking booking)
        {
            string operatorEmail = "operator@example.com"; // Get this from your settings or database
            string subject = "New Booking Assigned";
            string body = $"<p>A new booking has been assigned to you for <strong>Machine ID {booking.MachineId}</strong> on <strong>{booking.Date.ToShortDateString()}</strong>.</p><p>Please prepare for the job.</p>";

            await _emailService.SendEmailAsync(operatorEmail, subject, body);
        }

        public IActionResult Confirmation(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        private string GetLoggedInUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        //NEW
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .Select(b => new BookingVM
                {
                    Id = b.Id,
                    CustomerName = b.User.Name,  // Assuming AppUser is related to Booking
                    CustomerEmail = b.User.Email,
                    MachineName = b.Machine.Name,  // Assuming Machine is related to Booking
                    Date = b.Date,
                    Status = b.Status,
                    IsAcknowledged = b.IsAcknowledged,
                    IsCompleted = b.IsCompleted
                }).ToListAsync();

            return View(bookings);
        }


        public async Task<IActionResult> Conflicts()
        {
            // Fetch conflicts that are not resolved (filtering out resolved conflicts)
            var conflicts = await _context.Conflicts
                .Include(c => c.User) // Include user information
                .Where(c => !c.Status.Contains("Resolved")) // Only fetch unresolved conflicts
                .ToListAsync();

            var machineIds = conflicts.Select(c => c.MachineId).Distinct().ToList();
            var machines = await _context.Machines
                .Where(m => machineIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name);

            var conflictViewModels = conflicts.Select(c => new ConflictVM
            {
                Id = c.Id,
                UserId = c.UserId,
                CustomerName = c.User.Name,
                CustomerEmail = c.User.Email,
                CustomerAddress = c.User.Address,
                MachineId = c.MachineId,
                MachineName = machines.TryGetValue(c.MachineId, out var machineName) ? machineName : "Unknown",
                RequestedDate = c.RequestedDate,
                Status = c.Status,
                AlternativeMachineName = "PowerCutter 112" // Add logic to fetch alternative machines if needed
            }).ToList();

            return View(conflictViewModels);
        }




    }
}
