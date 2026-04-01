using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Infrastructure;

public sealed class MyWorkItemDbContext(DbContextOptions<MyWorkItemDbContext> options) : DbContext(options)
{
    public DbSet<WorkItemEntity> WorkItems => Set<WorkItemEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<UserWorkItemStatusEntity> UserWorkItemStatuses => Set<UserWorkItemStatusEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkItemEntity>(entity =>
        {
            entity.ToTable("WorkItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(100);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserWorkItemStatusEntity>(entity =>
        {
            entity.ToTable("UserWorkItemStatuses");
            entity.HasKey(x => new { x.UserId, x.WorkItemId });
            entity.Property(x => x.UserId).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.WorkItemStatuses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.WorkItem)
                .WithMany(x => x.UserStatuses)
                .HasForeignKey(x => x.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
