using System.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Model;


namespace TaskManagerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {

    }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
                    .Property(t => t.Title)
                    .HasMaxLength(200)
                    .IsRequired();

        modelBuilder.Entity<TaskItem>()
                    .Property(t => t.Description)
                    .HasMaxLength(500);

        modelBuilder.Entity<TaskItem>()
                    .Property(t => t.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");


    }
}