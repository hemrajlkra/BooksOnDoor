using BooksOnDoorWeb.Data;
using BooksOnDoorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BooksOnDoorWeb.Controllers
{
	public class CategoryController : Controller
	{
		private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
			_db = db;
        }
        public IActionResult Index()
		{
			List<Category> objListCategory = _db.Categories.ToList(); 
			return View(objListCategory);
		}
	}
}
