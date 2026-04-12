using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Model;


namespace TaskManagerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {

    }
    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<User> Users { get; set; }

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

        modelBuilder.Entity<TaskItem>()
                    .HasOne(t => t.User)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
        .Property(t => t.Email)
        .IsRequired()
        .HasMaxLength(50);

        modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

        modelBuilder.Entity<User>()
        .Property(u => u.PasswordHash)
        .IsRequired()
        .HasMaxLength(100);

        modelBuilder.Entity<User>()
        .Property(u => u.Role)
        .HasConversion<string>()
        .HasMaxLength(20);
    }
}