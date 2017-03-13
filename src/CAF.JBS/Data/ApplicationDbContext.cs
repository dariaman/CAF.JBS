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
            builder.Entity<CardIssuerBankModel>().ToTable("card_issuer_bank");
        }

        public DbSet<CardIssuerBankModel> CardIssuerBankModel { get; set; }
        public DbSet<cctypeModel> cctypeModel { get; set; }
    }
}
