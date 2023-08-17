using BooksOnDoor.Models.Models;
using BooksOnDoorWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksOnDoorWeb.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}
		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Name = "Sci-fi", DisplayOrder = 1, },
				new Category { Id = 2, Name = "Bollywood", DisplayOrder = 2 }
				);
		}
	}
}
