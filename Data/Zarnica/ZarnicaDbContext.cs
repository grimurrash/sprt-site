using Microsoft.EntityFrameworkCore;
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
        public DbSet<DirectivesType> DirectivesTypes { get; set; }
        
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamCount> TeamCounts { get; set; }
        public DbSet<TeamDistrictDistribution> TeamDistrictDistributions { get; set; }
        public DbSet<DrivingProfessions> DrivingProfessionses { get; set; }
        
        public DbSet<MilitaryComissariat> MilitaryComissariats { get; set; }
        public DbSet<MilitaryUnit> MilitaryUnits { get; set; }
        public DbSet<MilitaryDistrict> MilitaryDistricts { get; set; }
        public DbSet<Predstav> Predstavs { get; set; }
        public DbSet<ArmyType> ArmyTypes { get; set; }
        
        public ZarnicaDbContext(DbContextOptions<ZarnicaDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        
    }
}