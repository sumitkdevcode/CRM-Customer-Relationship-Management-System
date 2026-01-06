using CRM___Customer_Relationship_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRM___Customer_Relationship_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<CrmTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customer configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.CompanyName);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
                
                entity.HasOne(c => c.AssignedTo)
                    .WithMany(u => u.Customers)
                    .HasForeignKey(c => c.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Contact configuration
            builder.Entity<Contact>(entity =>
            {
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => new { e.FirstName, e.LastName });
                
                entity.HasOne(c => c.Customer)
                    .WithMany(cu => cu.Contacts)
                    .HasForeignKey(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Lead configuration
            builder.Entity<Lead>(entity =>
            {
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Source);
                
                entity.HasOne(l => l.AssignedTo)
                    .WithMany(u => u.Leads)
                    .HasForeignKey(l => l.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(l => l.ConvertedCustomer)
                    .WithMany()
                    .HasForeignKey(l => l.ConvertedCustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Deal configuration
            builder.Entity<Deal>(entity =>
            {
                entity.HasIndex(e => e.Stage);
                entity.HasIndex(e => e.ExpectedCloseDate);
                
                entity.HasOne(d => d.Customer)
                    .WithMany(c => c.Deals)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AssignedTo)
                    .WithMany(u => u.Deals)
                    .HasForeignKey(d => d.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Task configuration
            builder.Entity<CrmTask>(entity =>
            {
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                
                entity.HasOne(t => t.AssignedTo)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Customer)
                    .WithMany(c => c.Tasks)
                    .HasForeignKey(t => t.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Contact)
                    .WithMany(c => c.Tasks)
                    .HasForeignKey(t => t.ContactId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Lead)
                    .WithMany(l => l.Tasks)
                    .HasForeignKey(t => t.LeadId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Deal)
                    .WithMany(d => d.Tasks)
                    .HasForeignKey(t => t.DealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Note configuration
            builder.Entity<Note>(entity =>
            {
                entity.HasIndex(e => e.IsPinned);
                
                entity.HasOne(n => n.CreatedBy)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(n => n.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Customer)
                    .WithMany(c => c.Notes)
                    .HasForeignKey(n => n.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Contact)
                    .WithMany(c => c.ContactNotes)
                    .HasForeignKey(n => n.ContactId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.Lead)
                    .WithMany(l => l.Notes)
                    .HasForeignKey(n => n.LeadId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.Deal)
                    .WithMany(d => d.Notes)
                    .HasForeignKey(n => n.DealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Activity configuration
            builder.Entity<Activity>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.EntityType);
                
                entity.HasOne(a => a.PerformedBy)
                    .WithMany(u => u.Activities)
                    .HasForeignKey(a => a.PerformedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Customer)
                    .WithMany(c => c.Activities)
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Lead)
                    .WithMany(l => l.Activities)
                    .HasForeignKey(a => a.LeadId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Deal)
                    .WithMany(d => d.Activities)
                    .HasForeignKey(a => a.DealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
