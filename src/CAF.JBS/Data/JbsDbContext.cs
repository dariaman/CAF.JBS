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
        public JbsDbContext(DbContextOptions<JbsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<cctypeModel>().ToTable("cctype");
            builder.Entity<BankModel>().ToTable("bank");
            builder.Entity<BillingModel>().ToTable("billing");
            builder.Entity<prefixcardModel>().ToTable("prefixcard");
            builder.Entity<BillingHoldModel>().ToTable("billinghold");
            builder.Entity<PolicyBillingModel>().ToTable("policy_billing");
        }

        public DbSet<cctypeModel> cctypeModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
        public DbSet<BillingModel> BillingModel { get; set; }
        public DbSet<prefixcardModel> prefixcardModel { get; set; }
        public DbSet<BillingHoldModel> BillingHoldModel { get; set; }
        public DbSet<PolicyBillingModel> PolicyBillingModel { get; set; }
    }
}
