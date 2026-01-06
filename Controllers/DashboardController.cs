using CRM___Customer_Relationship_Management_System.Data;
using CRM___Customer_Relationship_Management_System.Models;
using CRM___Customer_Relationship_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM___Customer_Relationship_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var sixMonthsAgo = now.AddMonths(-6);

            var viewModel = new DashboardViewModel
            {
                // Customer stats
                TotalCustomers = await _context.Customers.CountAsync(),
                NewCustomersThisMonth = await _context.Customers
                    .CountAsync(c => c.CreatedAt >= startOfMonth),

                // Lead stats
                TotalLeads = await _context.Leads.CountAsync(),
                NewLeadsThisMonth = await _context.Leads
                    .CountAsync(l => l.CreatedAt >= startOfMonth),
                NewLeads = await _context.Leads
                    .CountAsync(l => l.Status == LeadStatus.New),
                QualifiedLeads = await _context.Leads
                    .CountAsync(l => l.Status == LeadStatus.Qualified),
                ConvertedLeads = await _context.Leads
                    .CountAsync(l => l.Status == LeadStatus.Converted),

                // Deal stats
                TotalDeals = await _context.Deals.CountAsync(),
                TotalDealValue = await _context.Deals.SumAsync(d => d.Value),
                ClosedWonValue = await _context.Deals
                    .Where(d => d.Stage == DealStage.ClosedWon)
                    .SumAsync(d => d.Value),
                PipelineValue = await _context.Deals
                    .Where(d => d.Stage != DealStage.ClosedWon && d.Stage != DealStage.ClosedLost)
                    .SumAsync(d => d.Value),
                WeightedPipelineValue = await _context.Deals
                    .Where(d => d.Stage != DealStage.ClosedWon && d.Stage != DealStage.ClosedLost)
                    .SumAsync(d => d.Value * d.Probability / 100),
                DealsClosedThisMonth = await _context.Deals
                    .CountAsync(d => d.ActualCloseDate >= startOfMonth && d.Stage == DealStage.ClosedWon),
                RevenueThisMonth = await _context.Deals
                    .Where(d => d.ActualCloseDate >= startOfMonth && d.Stage == DealStage.ClosedWon)
                    .SumAsync(d => d.Value),

                // Task stats
                PendingTasks = await _context.Tasks
                    .CountAsync(t => t.Status == Models.TaskStatus.Pending || t.Status == Models.TaskStatus.InProgress),
                OverdueTasks = await _context.Tasks
                    .CountAsync(t => t.Status != Models.TaskStatus.Completed && t.DueDate < now),

                // Deals by stage
                DealsByStage = await _context.Deals
                    .GroupBy(d => d.Stage)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),

                ValueByStage = await _context.Deals
                    .GroupBy(d => d.Stage)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Sum(d => d.Value)),

                // Leads by source
                LeadsBySource = await _context.Leads
                    .GroupBy(l => l.Source)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),

                // Recent activities
                RecentActivities = await _context.Activities
                    .Include(a => a.PerformedBy)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync(),

                // Upcoming tasks
                UpcomingTasks = await _context.Tasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Customer)
                    .Where(t => t.Status != Models.TaskStatus.Completed && t.DueDate >= now)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .ToListAsync(),

                // Recent deals
                RecentDeals = await _context.Deals
                    .Include(d => d.Customer)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                // Recent leads
                RecentLeads = await _context.Leads
                    .Include(l => l.AssignedTo)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                // Top customers
                TopCustomers = await _context.Customers
                    .Select(c => new CustomerSummary
                    {
                        CustomerId = c.Id,
                        CompanyName = c.CompanyName,
                        TotalDealValue = c.Deals.Where(d => d.Stage == DealStage.ClosedWon).Sum(d => d.Value),
                        DealCount = c.Deals.Count
                    })
                    .OrderByDescending(c => c.TotalDealValue)
                    .Take(5)
                    .ToListAsync()
            };

            // Calculate lead conversion rate
            var totalLeadsConverted = await _context.Leads.CountAsync(l => l.Status == LeadStatus.Converted);
            var totalLeadsProcessed = await _context.Leads.CountAsync(l => l.Status != LeadStatus.New);
            viewModel.LeadConversionRate = totalLeadsProcessed > 0 
                ? Math.Round((double)totalLeadsConverted / totalLeadsProcessed * 100, 1) 
                : 0;

            // Monthly sales data for chart
            var salesData = await _context.Deals
                .Where(d => d.ActualCloseDate >= sixMonthsAgo && d.Stage == DealStage.ClosedWon)
                .GroupBy(d => new { d.ActualCloseDate!.Value.Year, d.ActualCloseDate!.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Value = g.Sum(d => d.Value),
                    Count = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            viewModel.MonthlySales = salesData.Select(s => new MonthlyData
            {
                Month = $"{s.Year}-{s.Month:D2}",
                Value = s.Value,
                Count = s.Count
            }).ToList();

            // Monthly leads data for chart
            var leadsData = await _context.Leads
                .Where(l => l.CreatedAt >= sixMonthsAgo)
                .GroupBy(l => new { l.CreatedAt.Year, l.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            viewModel.MonthlyLeads = leadsData.Select(s => new MonthlyData
            {
                Month = $"{s.Year}-{s.Month:D2}",
                Count = s.Count
            }).ToList();

            return View(viewModel);
        }
    }
}
