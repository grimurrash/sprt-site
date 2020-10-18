using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App.Models;

namespace NewSprt.Data.App
{
    public class AppDbContext : DbContext
    {
        //migratiins add command:
        //InitialCreate
        //dotnet ef migrations add InitialCreate --context NewSprt.Data.App.AppDbContext

        //database update command:
        //dotnet ef database update --context NewSprt.Data.App.AppDbContext
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPermission>().HasKey(t => new {t.PermissionId, t.UserId});
        }
    }
}