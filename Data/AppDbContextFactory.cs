using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace AreaGourmet.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseMySql("Server=localhost;Database=area_gourmet;User=gourmet;Password=design-time-only", new MySqlServerVersion(new Version(8, 0, 36)));
        return new AppDbContext(options.Options);
    }
}
