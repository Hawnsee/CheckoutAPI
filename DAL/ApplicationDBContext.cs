
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace DAL
{

    public class ApplicationDBContext : DbContext
    {
        public DbSet<IdempotentRequest> IdempotentRequest { get; set; }

        public DbSet<CheckoutState> CheckoutState { get; set; }

        public DbSet<CheckoutOrder> CheckoutOrder { get; set; }


        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<IdempotentRequest>().HasKey(i => i.Key);

            modelBuilder.Entity<IdempotentRequest>()
                .Property(i => i.Key)
                .HasMaxLength(100)
                .ValueGeneratedNever();

            modelBuilder.Entity<CheckoutState>().HasKey(c => c.CorrelationId);

            modelBuilder.Entity<CheckoutState>()
                .Property(i => i.CorrelationId)
                .HasMaxLength(100)
                .ValueGeneratedNever();

            modelBuilder.Entity<CheckoutState>()
                .Property(i => i.PaymentResult)
                .IsRequired(false);

            modelBuilder.Entity<CheckoutOrder>().HasKey(c => c.Id);

            modelBuilder.Entity<CheckoutOrder>()
                .Property(i => i.Id)
                .HasMaxLength(100)
                .ValueGeneratedNever();

            modelBuilder.Entity<CheckoutOrder>()
                .Property(i => i.Price)
                .HasPrecision(10, 2);
        }
    }
}