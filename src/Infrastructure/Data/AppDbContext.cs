using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members");
            entity.HasKey(p => p.Id);
            entity.Property(m => m.Username).HasColumnName("username").IsRequired().HasMaxLength(30);
            entity.Property(m => m.Password).HasColumnName("password").IsRequired();
            entity.Property(m => m.Email).HasColumnName("email").IsRequired();
        });
    }
}
