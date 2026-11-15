using Microsoft.EntityFrameworkCore;

namespace IdempotentApi_Payment_Duplication.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
        public DbSet<Order> Orders => Set<Order>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdempotencyRecord>(entity =>
            {
                entity.HasKey(e => e.Key);//configured Key as the primary key (entity.HasKey(e => e.Key)) in your AppDbContext,
                                          //SQLite will reject the second simultaneous insert and throw a DbUpdateException

                entity.Property(e => e.Key).HasMaxLength(100);
                entity.Property(e => e.ResponseBody).HasColumnType("TEXT");
            });
        }
    }
}
