using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BooksOnDoorWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        [BindProperty]
        public ShopingCartVM shoppingCartVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            
            foreach(var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(shoppingCartVM);
        }
        public double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if (cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            else
            {
                if(cart.Count <= 100)
                {
                    return cart.Product.Price50;
                }
                else
                {
                    return cart.Product.Price100;
                }
            }
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.Id == cartId);
            cartFromDb.Count++;
            _unitOfWork.ShoppingCart.update(cartFromDb);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.Id==cartId);
            if(cartFromDb.Count<1 || cartFromDb.Count==0)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCart.update(cartFromDb);
            }
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.Application.Get(u=>u.Id== userId);

            shoppingCartVM.OrderHeader.Name=shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(shoppingCartVM);
            
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == userId,
                includeProperties: "Product");
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
				
			ApplicationUser applicationUser = _unitOfWork.Application.Get(u => u.Id == userId);
			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.save();
			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
                OrderDetails orderDetails = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Prize = cart.Price,
                    Count = cart.Count
                };
				_unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				
			}
			return RedirectToAction(nameof(OrderConfirmation),new {id=shoppingCartVM.OrderHeader.Id});

		}
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
	}
}
