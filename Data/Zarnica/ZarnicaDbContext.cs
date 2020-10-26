using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NewSprt.Data.Zarnica.Models;

namespace NewSprt.Data.Zarnica
{
    public class ZarnicaDbContext : DbContext
    {
        public DbSet<Recruit> Recruits { get; set; }
        public DbSet<AdditionalData> AdditionalDatas { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<Relative> Relatives { get; set; }
        public DbSet<EventControl> EventControls { get; set; }

        public DbSet<SpecialPerson> SpecialPersons { get; set; }
        public DbSet<SpecialPersonToRecruit> SpecialPersonToRecruits { get; set; }
        public DbSet<SpecialPersonToRequirement> SpecialPersonToRequirements { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<RequirementType> RequirementTypes { get; set; }
        public DbSet<DirectiveType> DirectivesTypes { get; set; }
        
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamCount> TeamCounts { get; set; }
        public DbSet<TeamDistrictDistribution> TeamDistrictDistributions { get; set; }
        public DbSet<DrivingProfessions> DrivingProfessionses { get; set; }
        
        public DbSet<MilitaryComissariat> MilitaryComissariats { get; set; }
        public DbSet<MilitaryUnit> MilitaryUnits { get; set; }
        public DbSet<MilitaryDistrict> MilitaryDistricts { get; set; }
        public DbSet<Predstav> Predstavs { get; set; }
        public DbSet<ArmyType> ArmyTypes { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpecialPersonToRecruit>()
                .HasKey(m => new {m.SpecialPersonId, m.RecruitId});
            modelBuilder.Entity<SpecialPersonToRequirement>()
                .HasKey(m => new {m.SpecialPersonId, m.RequirementId});

            modelBuilder.Entity<SpecialPerson>()
                .Property(m => m.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("nextval('public.gsp05_d_id_seq'::text)");
        }

        public ZarnicaDbContext(DbContextOptions<ZarnicaDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}