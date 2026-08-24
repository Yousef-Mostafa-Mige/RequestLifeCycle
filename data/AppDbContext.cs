using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.Enitities;

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
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RepairShop>()
                .HasOne(o => o.User)
                .WithOne(oi => oi.RepairShop)
                .HasForeignKey<RepairShop>(o => o.UserId);
            modelBuilder.Entity<RequestOffer>()
                .HasOne(o => o.ServiceRequest)
                .WithMany(oi => oi.requestOffers)
                .HasForeignKey(o => o.ServiceRequestId);
            modelBuilder.Entity<RequestOffer>()
                .HasOne(o => o.RepairShop)
                .WithMany(oi => oi.requestOffers)
                .HasForeignKey(o => o.RepairShopId);
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(o => o.Customer)
                .WithMany(oi => oi.servicerequest)
                .HasForeignKey(o => o.CustomerId);
        }
    }
}