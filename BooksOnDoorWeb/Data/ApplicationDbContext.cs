using BooksOnDoorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksOnDoorWeb.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}
		public DbSet<Category> Categories { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Name = "Sci-fi", DisplayOrder = 1},
				new Category { Id = 2, Name = "Bollywood", DisplayOrder = 2 }
				);
		}
	}
}
