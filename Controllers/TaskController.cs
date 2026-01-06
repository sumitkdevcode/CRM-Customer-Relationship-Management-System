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
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(Models.TaskStatus? status, TaskPriority? priority)
        {
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentPriority"] = priority;

            var tasks = _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Customer)
                .Include(t => t.Lead)
                .Include(t => t.Deal)
                .AsQueryable();

            if (status.HasValue)
                tasks = tasks.Where(t => t.Status == status);
            if (priority.HasValue)
                tasks = tasks.Where(t => t.Priority == priority);

            return View(await tasks.OrderBy(t => t.DueDate).ToListAsync());
        }

        public async Task<IActionResult> MyTasks()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var tasks = await _context.Tasks
                .Include(t => t.Customer)
                .Include(t => t.Lead)
                .Include(t => t.Deal)
                .Where(t => t.AssignedToId == currentUser.Id)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Customer)
                .Include(t => t.Lead)
                .Include(t => t.Deal)
                .FirstOrDefaultAsync(m => m.Id == id);

            return task == null ? NotFound() : View(task);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            var currentUser = await _userManager.GetUserAsync(User);
            var task = new CrmTask
            {
                DueDate = DateTime.UtcNow.AddDays(1),
                AssignedToId = currentUser?.Id ?? ""
            };
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrmTask task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedAt = DateTime.UtcNow;
                _context.Add(task);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(task);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            await PopulateDropdownsAsync();
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CrmTask task)
        {
            if (id != task.Id) return NotFound();

            if (ModelState.IsValid)
            {
                task.UpdatedAt = DateTime.UtcNow;
                if (task.Status == Models.TaskStatus.Completed && !task.CompletedDate.HasValue)
                    task.CompletedDate = DateTime.UtcNow;

                _context.Update(task);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync();
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Status = Models.TaskStatus.Completed;
            task.CompletedDate = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task completed!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var task = await _context.Tasks.Include(t => t.AssignedTo).FirstOrDefaultAsync(m => m.Id == id);
            return task == null ? NotFound() : View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
            ViewBag.Customers = new SelectList(await _context.Customers.OrderBy(c => c.CompanyName).ToListAsync(), "Id", "CompanyName");
            ViewBag.Leads = new SelectList(await _context.Leads.OrderBy(l => l.LastName).ToListAsync(), "Id", "FullName");
            ViewBag.Deals = new SelectList(await _context.Deals.OrderBy(d => d.Title).ToListAsync(), "Id", "Title");
        }
    }
}
