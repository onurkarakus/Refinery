using Microsoft.EntityFrameworkCore;
using Refinery.Core.Entities;

namespace Refinery.Infrastructure.Data;

public class RefineryDbContext: DbContext
{
    public RefineryDbContext(DbContextOptions<RefineryDbContext> options) : base(options)
    {

    }

    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>().HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);
    }
}
