using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class UserDbContext : IdentityDbContext<ApplicationUser>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("aspnetusers");
            builder.Entity<IdentityRole>().ToTable("aspnetroles");

            builder.Entity<IdentityUserRole<string>>().ToTable("aspnetuserroles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("aspnetuserclaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("aspnetuserlogins");

            builder.Entity<IdentityRoleClaim<string>>().ToTable("aspnetroleclaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("aspnetusertokens");

        }

    }
}
