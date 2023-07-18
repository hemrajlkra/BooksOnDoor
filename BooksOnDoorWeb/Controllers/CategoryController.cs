using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoorWeb.Data;
using BooksOnDoorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BooksOnDoorWeb.Controllers
{
    public class CategoryController : Controller
    {
        //private readonly ApplicationDbContext _db;
        private readonly ICategoryRepository _categoryRepository;
        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public IActionResult Index()
        {
            //List<Category> objListCategory = _db.Categories.ToList();
            List<Category> objListCategory = _categoryRepository.Getall().ToList();
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
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and Display Order doesn't Contain Same value.");

            }
            if (ModelState.IsValid)
            {
                //This statement will add the item on index
                //_db.Categories.Add(obj);
                _categoryRepository.Add(obj);
                //This statement will add the item to the database.
                //_db.SaveChanges();
                _categoryRepository.save();
                TempData["Success"] = "Category created Successfully";
                return RedirectToAction("Index");

            }
            return View();
        }
        public IActionResult Edit(int? Id)
        {
            //Category? categoryFromDb = _db.Categories.Find(Id);
            Category? categoryFromDb = _categoryRepository.Get(u=>u.Id == Id);

            //these are the two ways by which we can retrieve id.
            //Category? cateforyFromDb1 = _db.Categories.FirstOrDefault(id => id.Id ==Id);
            //Category? categoryFromDb2 = _db.Categories.Where(id => id.Id == Id).FirstOrDefault();
            if (Id == null || Id == 0 || categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                //_db.Categories.Update(category);
                _categoryRepository.update(category);
                //_db.SaveChanges();
                _categoryRepository.save();
                TempData["Success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Delete(int? Id)
        {
            //Category? categoryFromDb = _db.Categories.Find(Id);
            Category? categoryFromDb = _categoryRepository.Get(i=>i.Id == Id);
            if (Id == null || Id == 0 || categoryFromDb == null)
                return NotFound();
            return View(categoryFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteItem(int? id)
        {
            //Category? categoryFromDb = _db.Categories.Find(id);
            Category categoryFromDb = _categoryRepository.Get(i=>i.Id==id);
            //_db.Categories.Remove(categoryFromDb);
            _categoryRepository.Remove(categoryFromDb);
            //_db.SaveChanges();
            _categoryRepository.save();
            TempData["Success"] = "Category deleted Successfully";
            return RedirectToAction("Index");


        }
    }
}
