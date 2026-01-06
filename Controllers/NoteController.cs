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
    public class NoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NoteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? customerId, int? leadId, int? dealId)
        {
            var notes = _context.Notes
                .Include(n => n.CreatedBy)
                .Include(n => n.Customer)
                .Include(n => n.Lead)
                .Include(n => n.Deal)
                .AsQueryable();

            if (customerId.HasValue)
                notes = notes.Where(n => n.CustomerId == customerId);
            if (leadId.HasValue)
                notes = notes.Where(n => n.LeadId == leadId);
            if (dealId.HasValue)
                notes = notes.Where(n => n.DealId == dealId);

            return View(await notes.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Create(int? customerId, int? leadId, int? dealId)
        {
            await PopulateDropdownsAsync();
            var note = new Note();
            if (customerId.HasValue) note.CustomerId = customerId;
            if (leadId.HasValue) note.LeadId = leadId;
            if (dealId.HasValue) note.DealId = dealId;
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            note.CreatedById = currentUser.Id;
            note.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(note);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Note created successfully!";

                if (note.CustomerId.HasValue)
                    return RedirectToAction("Details", "Customer", new { id = note.CustomerId });
                if (note.LeadId.HasValue)
                    return RedirectToAction("Details", "Lead", new { id = note.LeadId });
                if (note.DealId.HasValue)
                    return RedirectToAction("Details", "Deal", new { id = note.DealId });

                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(note);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return NotFound();
            await PopulateDropdownsAsync();
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id) return NotFound();

            if (ModelState.IsValid)
            {
                note.UpdatedAt = DateTime.UtcNow;
                _context.Update(note);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Note updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Note deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return NotFound();

            note.IsPinned = !note.IsPinned;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Customers = new SelectList(await _context.Customers.OrderBy(c => c.CompanyName).ToListAsync(), "Id", "CompanyName");
            ViewBag.Leads = new SelectList(await _context.Leads.OrderBy(l => l.LastName).ToListAsync(), "Id", "FullName");
            ViewBag.Deals = new SelectList(await _context.Deals.OrderBy(d => d.Title).ToListAsync(), "Id", "Title");
        }
    }
}
