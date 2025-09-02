using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZuevUS.Mud.Database.Enum;
using ZuevUS.Mud.Database.Models;

namespace ZuevUS.Mud.Database;
public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile
            {
                UserId = "admin-seed-id",
                UserName = "Administrator",
                Email = "admin@zuevus.mud",
                Role = UserRole.Admin,
                CreatedDate = DateTime.UtcNow
            }
        );
    }
}
