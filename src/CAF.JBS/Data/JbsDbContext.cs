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

            builder.Entity<CustomerInfo>().ToTable("customer_info");
            builder.Entity<PolicyAc>().ToTable("policy_ac");
            builder.Entity<PolicyCc>().ToTable("policy_cc");
            builder.Entity<PolicyVa>().ToTable("policy_va");
            builder.Entity<PolicyLastTrans>().ToTable("policy_last_trans");
        }

        public DbSet<cctypeModel> cctypeModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
        public DbSet<BillingModel> BillingModel { get; set; }
        public DbSet<prefixcardModel> prefixcardModel { get; set; }
        public DbSet<BillingHoldModel> BillingHoldModel { get; set; }
        public DbSet<PolicyBillingModel> PolicyBillingModel { get; set; }
        public DbSet<CustomerInfo> CustomerInfo { get; set; }
        public DbSet<PolicyAc> PolicyAc { get; set; }
        public DbSet<PolicyCc> PolicyCc { get; set; }
        public DbSet<PolicyVa> PolicyVa { get; set; }
        public DbSet<PolicyLastTrans> PolicyLastTrans { get; set; }
    }
}
