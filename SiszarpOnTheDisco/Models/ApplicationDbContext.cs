using Microsoft.EntityFrameworkCore;
using SiszarpOnTheDisco.Models.Allergens;
using SiszarpOnTheDisco.Models.Lawn;
using SiszarpOnTheDisco.Models.MusicLinks;
using SmartEnum.EFCore;

namespace SiszarpOnTheDisco.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MusicLink> MusicLinks { get; set; }
    public DbSet<AllergenIcon> AllergenIcons { get; set; }

    public DbSet<LawnEvent> LawnEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ConfigureSmartEnum();
    }
}