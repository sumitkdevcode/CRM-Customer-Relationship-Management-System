using CRM___Customer_Relationship_Management_System.Data;
using CRM___Customer_Relationship_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM___Customer_Relationship_Management_System.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string searchString, CustomerStatus? status, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            var customers = _context.Customers
                .Include(c => c.AssignedTo)
                .Include(c => c.Contacts)
                .Include(c => c.Deals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.CompanyName.Contains(searchString) ||
                    (c.Email != null && c.Email.Contains(searchString)) ||
                    (c.City != null && c.City.Contains(searchString)));
            }

            if (status.HasValue)
            {
                customers = customers.Where(c => c.Status == status);
            }

            customers = sortOrder switch
            {
                "name_desc" => customers.OrderByDescending(c => c.CompanyName),
                "Date" => customers.OrderBy(c => c.CreatedAt),
                "date_desc" => customers.OrderByDescending(c => c.CreatedAt),
                _ => customers.OrderBy(c => c.CompanyName)
            };

            return View(await customers.ToListAsync());
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.AssignedTo)
                .Include(c => c.Contacts)
                .Include(c => c.Deals)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.CreatedBy)
                .Include(c => c.Tasks)
                .Include(c => c.Activities)
                    .ThenInclude(a => a.PerformedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.UtcNow;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                // Log activity
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LogActivity(ActivityType.Created, EntityType.Customer, customer.Id,
                        $"Created customer: {customer.CompanyName}", currentUser.Id, customer.Id);
                }

                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync();
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customer.UpdatedAt = DateTime.UtcNow;
                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        await LogActivity(ActivityType.Updated, EntityType.Customer, customer.Id,
                            $"Updated customer: {customer.CompanyName}", currentUser.Id, customer.Id);
                    }

                    TempData["Success"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

        private async Task PopulateDropdownsAsync()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
        }

        private async Task LogActivity(ActivityType type, EntityType entityType, int entityId,
            string description, string userId, int? customerId = null)
        {
            var activity = new Activity
            {
                Type = type,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                PerformedById = userId,
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }
    }
}
