#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Data;

namespace MovieApp.Tests.Services;

internal static class TestDbContextFactory
{
    public static MovieAppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<MovieAppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new MovieAppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }
}
