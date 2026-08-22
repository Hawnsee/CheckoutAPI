
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace DAL
{

    public class ApplicationDBContext : DbContext
    {
        public DbSet<IdempotentRequest> IdempotentRequest { get; set; }

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
        }
    }
}