using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class Life21pDbContext : IdentityDbContext<ApplicationUser>
    {
        public Life21pDbContext(DbContextOptions<Life21pDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Prospect>().ToTable("prospect");
            builder.Entity<Quote>().ToTable("quote");
            builder.Entity<QuoteCoverage>().ToTable("quote_coverage");
        }
        public DbSet<Prospect> Prospect { get; set; }
        public DbSet<Quote> Quote { get; set; }
        public DbSet<QuoteCoverage> QuoteCoverage { get; set; }

    }
}
