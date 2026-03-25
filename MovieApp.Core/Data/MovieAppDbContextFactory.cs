#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovieApp.Core.Data;

/// <summary>
/// Design-time factory for Entity Framework tooling.
/// </summary>
public class MovieAppDbContextFactory : IDesignTimeDbContextFactory<MovieAppDbContext>
{
    /// <inheritdoc />
    public MovieAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MovieAppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MovieAppDb;Trusted_Connection=True;");
        return new MovieAppDbContext(optionsBuilder.Options);
    }
}
