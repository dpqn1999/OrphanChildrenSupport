using Microsoft.EntityFrameworkCore;
using OrphanChildrenSupport.Services.Models;
using System;
using System.Linq;

namespace OrphanChildrenSupport.Infrastructure.Data
{
    public class OrphanChildrenSupportDbContext : DbContext
    {
        public OrphanChildrenSupportDbContext()
        {
        }

        public OrphanChildrenSupportDbContext(DbContextOptions options) : base(options)
        { }
        public DbSet<PersonalProfile> PersonalProfiles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.LogTo(Console.WriteLine);
        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            builder.Entity<PersonalProfile>().ToTable("PersonalProfile");
        }
    }
}
