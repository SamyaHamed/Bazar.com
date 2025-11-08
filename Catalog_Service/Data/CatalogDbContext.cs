using CATALOGSERVICE.Modele;
using Microsoft.EntityFrameworkCore;

namespace CATALOGSERVICE.Data
{
        public class CatalogDbContext : DbContext
        {
            public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

            public DbSet<Book> Books { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Book>().HasData(
                    new Book { Id = 1, Title = "How to get a good grade in DOS in 40 minutes a day", Topic = "distributed systems", Quantity = 5, Price = 45 },
                    new Book { Id = 2, Title = "RPCs for Noobs", Topic = "distributed systems", Quantity = 10, Price = 50 },
                    new Book { Id = 3, Title = "Xen and the Art of Surviving Undergraduate School", Topic = "undergraduate school", Quantity = 3, Price = 40 },
                    new Book { Id = 4, Title = "Cooking for the Impatient Undergrad", Topic = "undergraduate school", Quantity = 7, Price = 30 }
                );
            }
        }

    
}