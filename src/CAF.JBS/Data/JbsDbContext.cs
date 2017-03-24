using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class JbsDbContext : IdentityDbContext<ApplicationUser>
    {
        public JbsDbContext(DbContextOptions<JbsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<cctypeModel>().ToTable("cctype");
            builder.Entity<BankModel>().ToTable("bank");
        }

        public DbSet<cctypeModel> cctypeModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
    }
}
