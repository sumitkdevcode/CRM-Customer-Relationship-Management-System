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
    public class LeadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Lead
        public async Task<IActionResult> Index(string searchString, LeadStatus? status, LeadSource? source, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSource"] = source;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["RatingSortParm"] = sortOrder == "Rating" ? "rating_desc" : "Rating";

            var leads = _context.Leads
                .Include(l => l.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                leads = leads.Where(l =>
                    l.FirstName.Contains(searchString) ||
                    l.LastName.Contains(searchString) ||
                    l.Email.Contains(searchString) ||
                    (l.CompanyName != null && l.CompanyName.Contains(searchString)));
            }

            if (status.HasValue)
            {
                leads = leads.Where(l => l.Status == status);
            }

            if (source.HasValue)
            {
                leads = leads.Where(l => l.Source == source);
            }

            leads = sortOrder switch
            {
                "name_desc" => leads.OrderByDescending(l => l.LastName),
                "Date" => leads.OrderBy(l => l.CreatedAt),
                "date_desc" => leads.OrderByDescending(l => l.CreatedAt),
                "Rating" => leads.OrderBy(l => l.Rating),
                "rating_desc" => leads.OrderByDescending(l => l.Rating),
                _ => leads.OrderBy(l => l.LastName)
            };

            return View(await leads.ToListAsync());
        }

        // GET: Lead/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Leads
                .Include(l => l.AssignedTo)
                .Include(l => l.Notes)
                    .ThenInclude(n => n.CreatedBy)
                .Include(l => l.Tasks)
                .Include(l => l.Activities)
                    .ThenInclude(a => a.PerformedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lead == null)
            {
                return NotFound();
            }

            return View(lead);
        }

        // GET: Lead/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Lead/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lead lead)
        {
            if (ModelState.IsValid)
            {
                lead.CreatedAt = DateTime.UtcNow;
                _context.Add(lead);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LogActivity(ActivityType.Created, EntityType.Lead, lead.Id,
                        $"Created lead: {lead.FullName}", currentUser.Id, null, lead.Id);
                }

                TempData["Success"] = "Lead created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(lead);
        }

        // GET: Lead/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Leads.FindAsync(id);
            if (lead == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync();
            return View(lead);
        }

        // POST: Lead/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lead lead)
        {
            if (id != lead.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lead.UpdatedAt = DateTime.UtcNow;
                    _context.Update(lead);
                    await _context.SaveChangesAsync();

                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        await LogActivity(ActivityType.Updated, EntityType.Lead, lead.Id,
                            $"Updated lead: {lead.FullName}", currentUser.Id, null, lead.Id);
                    }

                    TempData["Success"] = "Lead updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeadExists(lead.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(lead);
        }

        // GET: Lead/Convert/5
        public async Task<IActionResult> Convert(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Leads.FindAsync(id);
            if (lead == null)
            {
                return NotFound();
            }

            if (lead.Status == LeadStatus.Converted)
            {
                TempData["Error"] = "This lead has already been converted.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(lead);
        }

        // POST: Lead/Convert/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertConfirmed(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead == null)
            {
                return NotFound();
            }

            // Create a new customer from the lead
            var customer = new Customer
            {
                CompanyName = lead.CompanyName ?? $"{lead.FirstName} {lead.LastName}",
                Industry = lead.Industry,
                Website = lead.Website,
                Phone = lead.Phone,
                Email = lead.Email,
                Address = lead.Address,
                City = lead.City,
                State = lead.State,
                Country = lead.Country,
                CustomerType = CustomerType.Customer,
                Status = CustomerStatus.Active,
                Description = lead.Description,
                AssignedToId = lead.AssignedToId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Create contact from lead
            var contact = new Contact
            {
                FirstName = lead.FirstName,
                LastName = lead.LastName,
                Email = lead.Email,
                Phone = lead.Phone,
                JobTitle = lead.JobTitle,
                CustomerId = customer.Id,
                IsPrimary = true,
                Address = lead.Address,
                City = lead.City,
                State = lead.State,
                Country = lead.Country,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);

            // Update lead status
            lead.Status = LeadStatus.Converted;
            lead.ConvertedAt = DateTime.UtcNow;
            lead.ConvertedCustomerId = customer.Id;
            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                await LogActivity(ActivityType.Converted, EntityType.Lead, lead.Id,
                    $"Converted lead {lead.FullName} to customer {customer.CompanyName}",
                    currentUser.Id, customer.Id, lead.Id);
            }

            TempData["Success"] = $"Lead converted to customer successfully! Customer ID: {customer.Id}";
            return RedirectToAction("Details", "Customer", new { id = customer.Id });
        }

        // GET: Lead/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lead = await _context.Leads
                .Include(l => l.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lead == null)
            {
                return NotFound();
            }

            return View(lead);
        }

        // POST: Lead/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead != null)
            {
                _context.Leads.Remove(lead);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lead deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LeadExists(int id)
        {
            return _context.Leads.Any(e => e.Id == id);
        }

        private async Task PopulateDropdownsAsync()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
        }

        private async Task LogActivity(ActivityType type, EntityType entityType, int entityId,
            string description, string userId, int? customerId = null, int? leadId = null)
        {
            var activity = new Activity
            {
                Type = type,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                PerformedById = userId,
                CustomerId = customerId,
                LeadId = leadId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }
    }
}
