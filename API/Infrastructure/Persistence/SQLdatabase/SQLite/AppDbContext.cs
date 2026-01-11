using Microsoft.EntityFrameworkCore;
using API.Domain.Entities;
using API.Domain.Entities.User;
using API.Domain.Entities.Test;

namespace API.Infrastructure.Persistence.SQLdatabase.SQLite.AppDbContext;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupJoinOrder> GroupJoinOrders { get; set; }
    public DbSet<TestResult> TestResults { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>().
            Property(r => r.Role).
            HasConversion<string>();

        builder.Entity<TestResult>().
            Property(r => r.SecondarySubject1).
            HasConversion<string>();
            
        builder.Entity<TestResult>().
            Property(r => r.SecondarySubject2).
            HasConversion<string>();
    }
}