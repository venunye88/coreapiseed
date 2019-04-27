using System;
using System.Linq;
using CodeApiSeed.Models;
using CoreApiSeed.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreApiSeed.Data
{
    public class AppDbContext:IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<UserProfile>().HasIndex(q=>q.Name).IsUnique();
            //builder.Entity<AppSetting>().HasIndex(q => q.Name).IsUnique();
            builder.ApplyConfiguration(new UserProfileConfiguration());
            builder.ApplyConfiguration(new AppSettingConfiguration());

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(x=>x.State==EntityState.Added)
                .Select(x=>x.Entity)
                .OfType<IAuditable>())

            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.ModifiedAt = DateTime.UtcNow;
            }

            foreach (var entry in ChangeTracker.Entries()
                .Where(x=>x.State==EntityState.Modified)
                .Select(x=>x.Entity)
                .OfType<IAuditable>())
            {
                entry.ModifiedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }
    }
}
