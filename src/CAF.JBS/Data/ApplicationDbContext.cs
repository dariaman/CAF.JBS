using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.Entity<CardIssuerBankModel>( entity =>
            //{
            //    entity.HasOne(d => d.Bank)
            //});
            //builder.Entity<bankModel>().HasKey(;
            //builder.Entity<BankModel>().HasKey(c => c.bank_id);
            //builder.Ignore<IdentityUserLogin<string>>();
            //builder.Ignore<IdentityUserRole<string>>();
            //builder.Ignore<IdentityUserClaim<string>>();
            //builder.Ignore<IdentityUserToken<string>>();
            //builder.Ignore<IdentityUser<string>>();
            //builder.Ignore<ApplicationUser>();
            builder.Entity<CardIssuerBankModel>().ToTable("card_issuer_bank");
            builder.Entity<BankModel>().ToTable("bank");
            builder.Entity<cctypeModel>().ToTable("cctype");
        }

        public DbSet<CardIssuerBankModel> CardIssuerBankModel { get; set; }
        public DbSet<cctypeModel> cctypeModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
    }
}
