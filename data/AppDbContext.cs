using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.Enitities;
using RequestLifeCycle.Entities;

namespace RequestLifeCycle.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<RepairShop> RepairShops { get; set; }
        public DbSet<RequestOffer> RequestOffers { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceRequest>()
        .Property(r => r.Status)
        .HasConversion<string>();

            modelBuilder.Entity<RequestOffer>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // تحديد دقة الـ decimal لميوز بافعل في MySQL
            modelBuilder.Entity<ServiceRequest>()
                .Property(r => r.ProposedPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RequestOffer>()
                .Property(o => o.OfferedPrice)
                .HasPrecision(18, 2);
        }
    }
}