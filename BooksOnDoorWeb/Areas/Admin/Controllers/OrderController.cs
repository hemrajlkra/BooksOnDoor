using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;

namespace BooksOnDoorWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
			_unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int orderId)
        {
            OrderVM = new(){
                orderHeader=_unitOfWork.OrderHeader.Get(u=>u.Id==orderId,includeProperties:"ApplicationUser"),
                orderDetails=_unitOfWork.OrderDetails.Getall(u=>u.OrderHeaderId==orderId,includeProperties:"Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            //update each item by admin and employee when need to update.
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name= OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber= OrderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City= OrderVM.orderHeader.City;
            orderHeaderFromDb.State= OrderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode= OrderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.update(orderHeaderFromDb);
            _unitOfWork.save();
            TempData["success"] = "Order updated successfully";
            return RedirectToAction(nameof(Details),new {orderId=orderHeaderFromDb.Id}) ;
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
            _unitOfWork.save();
            TempData["success"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.orderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier=OrderVM.orderHeader.Carrier;
            orderHeader.OrderStatus=SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus==SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Today.AddDays(30);//DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.update(orderHeader);
            _unitOfWork.save();
            TempData["success"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin +"," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id== OrderVM.orderHeader.Id);
            if(orderHeader.PaymentStatus == SD.StatusApproved) 
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusCancelled, SD.StatusRefund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.save();
            TempData["success"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }
        [HttpPost]
        [ActionName(nameof(Details))]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.orderHeader= _unitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.orderHeader.Id,includeProperties: "ApplicationUser");
            OrderVM.orderDetails = _unitOfWork.OrderDetails.Getall(u => u.OrderHeaderId == OrderVM.orderHeader.Id, includeProperties: "Product");
            var domain = Request.Scheme+ "://"+Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var items in OrderVM.orderDetails)
            {
                var sessionItemList = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(items.Prize * 100),
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var services = new SessionService();
                Session session = services.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.save();
                }
            }
            return View(orderHeaderId);
        }
        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            //List<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.Getall(includeProperties:"ApplicationUser").ToList();
            IEnumerable<OrderHeader> objOrderHeaders;
            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)) 
            {
                objOrderHeaders = _unitOfWork.OrderHeader.Getall(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders = _unitOfWork.OrderHeader.Getall(u=>u.ApplicationUserId==userId,includeProperties: "ApplicationUser").ToList();
            }
			switch (status)
			{
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "complete":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = objOrderHeaders });
		}
		#endregion
	}
}
