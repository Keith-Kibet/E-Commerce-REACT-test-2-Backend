using EcommApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommApp.Data
{
    public class EcommDbContext : IdentityDbContext
    {
        public EcommDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } // Add this line
        public DbSet<Cart> Carts { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var readerRoleId = "90af0538-a215-474e-a3bd-6134ede66631";
            var writerRoleId = "402fad9a-9696-40f2-a523-4f28db5be21c"; 


            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerRoleId,
                    ConcurrencyStamp = readerRoleId,
                    Name  = "Reader",
                    NormalizedName = "Reader".ToUpper()
                },

                new IdentityRole
                {
                    Id = writerRoleId,
                    ConcurrencyStamp = writerRoleId,
                    Name = "Writer",
                    NormalizedName = "Writer".ToUpper()
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);


        }

    }
}
