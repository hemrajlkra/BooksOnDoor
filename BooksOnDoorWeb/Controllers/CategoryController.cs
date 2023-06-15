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
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Category obj)
		{
			if (ModelState.IsValid)
			{
				//This statement will add the item on index
				_db.Categories.Add(obj);
				//This statement will add the item to the database.
				_db.SaveChanges();
				return RedirectToAction("Index");

			}
			return View();
		}
	}
}
