using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App.Models;

namespace NewSprt.Data.App
{
    public sealed class AppDbContext : DbContext
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
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<ConscriptionPeriod> ConscriptionPeriods { get; set; }
        public DbSet<MilitaryComissariat> MilitaryComissariats { get; set; }
        public DbSet<DactyloscopyStatus> DactyloscopyStatuses { get; set; }
        public DbSet<Recruit> Recruits { get; set; }
        public DbSet<Dismissal> Dismissals { get; set; }
        

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPermission>().HasKey(t => new {t.PermissionId, t.UserId});
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseModel && (
                e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entityEntry in entries)
            {
                ((BaseModel)entityEntry.Entity).UpdateDate = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseModel)entityEntry.Entity).CreateDate = DateTime.Now;
                }
            }
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseModel && (
                e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entityEntry in entries)
            {
                ((BaseModel)entityEntry.Entity).UpdateDate = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseModel)entityEntry.Entity).CreateDate = DateTime.Now;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}