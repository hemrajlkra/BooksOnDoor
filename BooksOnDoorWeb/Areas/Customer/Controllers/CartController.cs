﻿using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BooksOnDoorWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        [BindProperty]
        public ShopingCartVM shoppingCartVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
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
            IEnumerable<ProductImage> prodImages = _unitOfWork.ProductImage.Getall();
            foreach(var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = _unitOfWork.ProductImage.Getall(u => u.ProductId == cart.ProductId).ToList();
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
            //HttpContext.Session.SetInt32(SD.SessionCart,
            //        _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() +1);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.Id==cartId,tracked:true);
            if(cartFromDb.Count<=1 )
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCart.update(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count());
            }
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.Getall(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
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
                //it is regular customer
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is company user net payment for 30 days
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
                //var domain = "https://localhost:44309/";
                var domain = Request.Scheme +"://"+Request.Host.Value+"/";

                var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl= domain +$"customer/cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};
                foreach(var items in shoppingCartVM.ShoppingCartList)
                {
                    var sessionItemList = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(items.Price*100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = items.Product.Title
                            }
                        },
                        Quantity = items.Count
                    };
                    options.LineItems.Add(sessionItemList);
                }
				var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(shoppingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
                _unitOfWork.save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
				//service.Create(options);
			}
			return RedirectToAction(nameof(OrderConfirmation),new {id=shoppingCartVM.OrderHeader.Id});

		}
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id== id,includeProperties:"ApplicationUser");
            if(orderHeader.PaymentStatus!=SD.PaymentStatusDelayedPayment)
            {
                var services = new SessionService();
                Session session = services.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id,session.Id,session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.save();
                }
                HttpContext.Session.Clear();
            }
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "Wow!! New Order Placed - BooksOnDoor", $"Order Id-{orderHeader.Id}");
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.Getall(
                u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.save();
            return View(id);
        }
	}
}
