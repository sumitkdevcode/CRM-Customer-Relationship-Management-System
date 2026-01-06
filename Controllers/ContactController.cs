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
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, int? customerId)
        {
            ViewData["CurrentFilter"] = searchString;
            var contacts = _context.Contacts.Include(c => c.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                contacts = contacts.Where(c =>
                    c.FirstName.Contains(searchString) ||
                    c.LastName.Contains(searchString) ||
                    c.Email.Contains(searchString));
            }

            if (customerId.HasValue)
                contacts = contacts.Where(c => c.CustomerId == customerId);

            ViewBag.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.CompanyName).ToListAsync(),
                "Id", "CompanyName", customerId);

            return View(await contacts.OrderBy(c => c.LastName).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.Contacts
                .Include(c => c.Customer)
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);

            return contact == null ? NotFound() : View(contact);
        }

        public async Task<IActionResult> Create(int? customerId)
        {
            await PopulateDropdownsAsync();
            if (customerId.HasValue) ViewBag.SelectedCustomerId = customerId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.UtcNow;
                if (!await _context.Contacts.AnyAsync(c => c.CustomerId == contact.CustomerId))
                    contact.IsPrimary = true;

                _context.Add(contact);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contact created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(contact);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();
            await PopulateDropdownsAsync();
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (id != contact.Id) return NotFound();

            if (ModelState.IsValid)
            {
                contact.UpdatedAt = DateTime.UtcNow;
                _context.Update(contact);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contact updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(contact);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var contact = await _context.Contacts.Include(c => c.Customer).FirstOrDefaultAsync(m => m.Id == id);
            return contact == null ? NotFound() : View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contact deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Customers = new SelectList(
                await _context.Customers.Where(c => c.Status == CustomerStatus.Active).OrderBy(c => c.CompanyName).ToListAsync(),
                "Id", "CompanyName");
        }
    }
}
