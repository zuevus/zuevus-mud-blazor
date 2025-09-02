using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ZuevUS.Mud.Database;

public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
{
    public DBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DBContext>();

        // For design-time operations (migrations)
        _ = optionsBuilder.UseSqlite();

        return new DBContext(optionsBuilder.Options);
    }
}
