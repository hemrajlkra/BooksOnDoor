using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoorWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BooksOnDoorWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;

        }

        public IActionResult Index()
        {
            IEnumerable<Product> prodList = _unitOfWork.Product.Getall(includeProperties:"Category");
            return View(prodList);
        }
        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}