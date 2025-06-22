using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SiszarpOnTheDisco.Models;

// TODO: refactor this to not use appSettings as they are not available in current config. Or do some reading if we can get rid of this. 
internal class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {

        DbContextOptionsBuilder<ApplicationDbContext> optionBuilder = new();

        optionBuilder.UseNpgsql(GetConnectionString());
        return new ApplicationDbContext(optionBuilder.Options);
    }

    private static string GetConnectionString()
    {
        Console.WriteLine(Environment.GetEnvironmentVariable("$PG_HOST"));
        return $"Server={Environment.GetEnvironmentVariable("PG_HOST")};Port={Environment.GetEnvironmentVariable("PG_PORT")};Database={Environment.GetEnvironmentVariable("PG_DATABASE")};Username={Environment.GetEnvironmentVariable("PG_USERNAME")};Password={Environment.GetEnvironmentVariable("PG_PASSWORD")}";
    }

}