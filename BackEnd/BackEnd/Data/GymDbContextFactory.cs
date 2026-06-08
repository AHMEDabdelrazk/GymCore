using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymCore.API.Data;

public class GymDbContextFactory : IDesignTimeDbContextFactory<GymDbContext>
{
    public GymDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GymDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=GymCoreDb;Trusted_Connection=True;TrustServerCertificate=True");

        return new GymDbContext(optionsBuilder.Options);
    }
}