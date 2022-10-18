using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SiszarpOnTheDisco.Models;

// TODO: refactor this to not use appSettings as they are not available in current config. Or do some reading if we can get rid of this. 
internal class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json", false, true)
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        DbContextOptionsBuilder<ApplicationDbContext> optionBuilder = new();

        optionBuilder.UseNpgsql(connectionString);
        return new ApplicationDbContext(optionBuilder.Options);
    }
}