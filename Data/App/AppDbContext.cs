using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App.Models;

namespace NewSprt.Data.App
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}