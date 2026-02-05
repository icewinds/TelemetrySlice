using Microsoft.EntityFrameworkCore;

namespace TelemetrySlice.Api.Data;

public class TelemetryDbContext : DbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.CustomerId);
        });

        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => new { e.CustomerId, e.DeviceId });
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.DeviceId).IsRequired();
            entity.Property(e => e.Label).IsRequired();
            entity.Property(e => e.Location).IsRequired();
            
            entity.HasOne(d => d.Customer)
                .WithMany(c => c.Devices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.CustomerId);
        });

        // TelemetryEvent configuration
        modelBuilder.Entity<TelemetryEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.DeviceId).IsRequired();
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Unit).IsRequired();
            
            // Create unique constraint on EventId to prevent duplicates
            entity.HasIndex(e => e.EventId).IsUnique();
            
            // Create composite index for efficient queries
            entity.HasIndex(e => new { e.CustomerId, e.DeviceId, e.RecordedAt });
            
            entity.HasOne(t => t.Device)
                .WithMany(d => d.TelemetryEvents)
                .HasForeignKey(t => new { t.CustomerId, t.DeviceId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
