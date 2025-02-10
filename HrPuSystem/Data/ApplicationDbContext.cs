using HrPuSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrPuSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRecord> LeaveRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed LeaveType data (optional initial setup)
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { LeaveTypeId = 1, Name = "Maternity", DefaultDays = 70, IsPaid = true },
                new LeaveType { LeaveTypeId = 2, Name = "Paternity", DefaultDays = 1, IsPaid = true },
                new LeaveType { LeaveTypeId = 3, Name = "Annual", DefaultDays = 15, IsPaid = true },
                new LeaveType { LeaveTypeId = 4, Name = "Sick", DefaultDays = 3, IsPaid = true },
                new LeaveType { LeaveTypeId = 5, Name = "Unpaid", DefaultDays = 0, IsPaid = false },
                new LeaveType { LeaveTypeId = 6, Name = "Authorized", DefaultDays = 1, IsPaid = true }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}

