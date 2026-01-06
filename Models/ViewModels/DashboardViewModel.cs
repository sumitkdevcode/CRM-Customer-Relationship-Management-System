namespace CRM___Customer_Relationship_Management_System.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary statistics
        public int TotalCustomers { get; set; }
        public int TotalLeads { get; set; }
        public int TotalDeals { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        
        // Financial metrics
        public decimal TotalDealValue { get; set; }
        public decimal ClosedWonValue { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal WeightedPipelineValue { get; set; }
        
        // Monthly comparison
        public int NewCustomersThisMonth { get; set; }
        public int NewLeadsThisMonth { get; set; }
        public int DealsClosedThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        
        // Lead metrics
        public int NewLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public double LeadConversionRate { get; set; }
        
        // Deal stage breakdown
        public Dictionary<string, int> DealsByStage { get; set; } = new();
        public Dictionary<string, decimal> ValueByStage { get; set; } = new();
        
        // Lead source breakdown
        public Dictionary<string, int> LeadsBySource { get; set; } = new();
        
        // Recent activities
        public List<Activity> RecentActivities { get; set; } = new();
        
        // Upcoming tasks
        public List<CrmTask> UpcomingTasks { get; set; } = new();
        
        // Recent deals
        public List<Deal> RecentDeals { get; set; } = new();
        
        // Recent leads
        public List<Lead> RecentLeads { get; set; } = new();
        
        // Top customers by deal value
        public List<CustomerSummary> TopCustomers { get; set; } = new();
        
        // Monthly trend data for charts
        public List<MonthlyData> MonthlySales { get; set; } = new();
        public List<MonthlyData> MonthlyLeads { get; set; } = new();
    }

    public class CustomerSummary
    {
        public int CustomerId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalDealValue { get; set; }
        public int DealCount { get; set; }
    }

    public class MonthlyData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Count { get; set; }
    }
}
