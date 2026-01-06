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
    public class DealController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DealController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Deal
        public async Task<IActionResult> Index(string searchString, DealStage? stage, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStage"] = stage;
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["ValueSortParm"] = sortOrder == "Value" ? "value_desc" : "Value";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            var deals = _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                deals = deals.Where(d =>
                    d.Title.Contains(searchString) ||
                    (d.Customer != null && d.Customer.CompanyName.Contains(searchString)));
            }

            if (stage.HasValue)
            {
                deals = deals.Where(d => d.Stage == stage);
            }

            deals = sortOrder switch
            {
                "title_desc" => deals.OrderByDescending(d => d.Title),
                "Value" => deals.OrderBy(d => d.Value),
                "value_desc" => deals.OrderByDescending(d => d.Value),
                "Date" => deals.OrderBy(d => d.ExpectedCloseDate),
                "date_desc" => deals.OrderByDescending(d => d.ExpectedCloseDate),
                _ => deals.OrderBy(d => d.Title)
            };

            return View(await deals.ToListAsync());
        }

        // GET: Deal/Pipeline
        public async Task<IActionResult> Pipeline()
        {
            var deals = await _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedTo)
                .Where(d => d.Stage != DealStage.ClosedWon && d.Stage != DealStage.ClosedLost)
                .OrderBy(d => d.Stage)
                .ThenByDescending(d => d.Value)
                .ToListAsync();

            var groupedDeals = deals.GroupBy(d => d.Stage)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(groupedDeals);
        }

        // GET: Deal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deal = await _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedTo)
                .Include(d => d.Notes)
                    .ThenInclude(n => n.CreatedBy)
                .Include(d => d.Tasks)
                .Include(d => d.Activities)
                    .ThenInclude(a => a.PerformedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deal == null)
            {
                return NotFound();
            }

            return View(deal);
        }

        // GET: Deal/Create
        public async Task<IActionResult> Create(int? customerId)
        {
            await PopulateDropdownsAsync();
            
            if (customerId.HasValue)
            {
                ViewBag.SelectedCustomerId = customerId;
            }

            return View();
        }

        // POST: Deal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Deal deal)
        {
            if (ModelState.IsValid)
            {
                deal.CreatedAt = DateTime.UtcNow;
                deal.Probability = GetDefaultProbability(deal.Stage);
                _context.Add(deal);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await LogActivity(ActivityType.Created, EntityType.Deal, deal.Id,
                        $"Created deal: {deal.Title}", currentUser.Id, deal.CustomerId, null, deal.Id);
                }

                TempData["Success"] = "Deal created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(deal);
        }

        // GET: Deal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync();
            return View(deal);
        }

        // POST: Deal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Deal deal)
        {
            if (id != deal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDeal = await _context.Deals.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    var stageChanged = existingDeal != null && existingDeal.Stage != deal.Stage;

                    deal.UpdatedAt = DateTime.UtcNow;

                    // Set actual close date if deal is closed
                    if ((deal.Stage == DealStage.ClosedWon || deal.Stage == DealStage.ClosedLost) 
                        && !deal.ActualCloseDate.HasValue)
                    {
                        deal.ActualCloseDate = DateTime.UtcNow;
                    }

                    _context.Update(deal);
                    await _context.SaveChangesAsync();

                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        var activityType = stageChanged
                            ? (deal.Stage == DealStage.ClosedWon ? ActivityType.DealWon
                                : deal.Stage == DealStage.ClosedLost ? ActivityType.DealLost
                                : ActivityType.StatusChanged)
                            : ActivityType.Updated;

                        await LogActivity(activityType, EntityType.Deal, deal.Id,
                            stageChanged
                                ? $"Deal stage changed to {deal.Stage}: {deal.Title}"
                                : $"Updated deal: {deal.Title}",
                            currentUser.Id, deal.CustomerId, null, deal.Id);
                    }

                    TempData["Success"] = "Deal updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DealExists(deal.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(deal);
        }

        // POST: Deal/UpdateStage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStage(int id, DealStage newStage)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            deal.Stage = newStage;
            deal.Probability = GetDefaultProbability(newStage);
            deal.UpdatedAt = DateTime.UtcNow;

            if (newStage == DealStage.ClosedWon || newStage == DealStage.ClosedLost)
            {
                deal.ActualCloseDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var activityType = newStage == DealStage.ClosedWon ? ActivityType.DealWon
                    : newStage == DealStage.ClosedLost ? ActivityType.DealLost
                    : ActivityType.StatusChanged;

                await LogActivity(activityType, EntityType.Deal, deal.Id,
                    $"Deal stage changed to {newStage}: {deal.Title}",
                    currentUser.Id, deal.CustomerId, null, deal.Id);
            }

            TempData["Success"] = $"Deal stage updated to {newStage}!";
            return RedirectToAction(nameof(Pipeline));
        }

        // GET: Deal/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deal = await _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deal == null)
            {
                return NotFound();
            }

            return View(deal);
        }

        // POST: Deal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal != null)
            {
                _context.Deals.Remove(deal);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Deal deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DealExists(int id)
        {
            return _context.Deals.Any(e => e.Id == id);
        }

        private int GetDefaultProbability(DealStage stage)
        {
            return stage switch
            {
                DealStage.Qualification => 10,
                DealStage.NeedsAnalysis => 20,
                DealStage.ValueProposition => 40,
                DealStage.DecisionMakers => 60,
                DealStage.Proposal => 75,
                DealStage.Negotiation => 90,
                DealStage.ClosedWon => 100,
                DealStage.ClosedLost => 0,
                _ => 10
            };
        }

        private async Task PopulateDropdownsAsync()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName");

            var customers = await _context.Customers
                .Where(c => c.Status == CustomerStatus.Active)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
            ViewBag.Customers = new SelectList(customers, "Id", "CompanyName");
        }

        private async Task LogActivity(ActivityType type, EntityType entityType, int entityId,
            string description, string userId, int? customerId = null, int? leadId = null, int? dealId = null)
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
                DealId = dealId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }
    }
}
