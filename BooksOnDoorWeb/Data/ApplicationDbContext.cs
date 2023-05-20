using Microsoft.EntityFrameworkCore;

namespace BooksOnDoorWeb.Data
{
	public class ApplicationDbContext:DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base()
		{

		}
	}
}
