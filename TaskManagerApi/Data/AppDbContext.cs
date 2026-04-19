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

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<TaskTag> TaskTags { get; set; }

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

        // modelBuilder.Entity<User>()
        // .Property(u => u.Role)
        // .HasConversion<string>()
        // .HasMaxLength(20);

        modelBuilder.Entity<UserRole>()
        .HasOne(ur => ur.User)
        .WithMany(u => u.Roles)
        .HasForeignKey(ur => ur.UserId)
        .HasPrincipalKey(u => u.Id)
        .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<UserRole>()
        .HasKey(ur => new { ur.UserId, ur.Role });

        modelBuilder.Entity<UserRole>()
        .Property(ur => ur.Role)
        .HasConversion<string>()
        .HasMaxLength(20);


        modelBuilder.Entity<Tag>()
        .HasKey(tag => tag.Id);

        modelBuilder.Entity<Tag>()
        .Property(tag => tag.TagName)
        .HasMaxLength(15);


        modelBuilder.Entity<Tag>()
        .HasIndex(tag => tag.TagName)
        .IsUnique();

        modelBuilder.Entity<TaskTag>()
        .HasKey(tt => new { tt.TaskId, tt.TagId });

        modelBuilder.Entity<TaskTag>()
        .HasOne(tt => tt.TaskItem)
        .WithMany(t => t.TaskTags)
        .HasForeignKey(tt => tt.TaskId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskTag>()
        .HasOne(tt => tt.Tag)
        .WithMany(t => t.TaskTags)
        .HasForeignKey(tt => tt.TagId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}