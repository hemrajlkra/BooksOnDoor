using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BooksOnDoorWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class ContactController : Controller
	{
		private readonly IMailService _mailService;
		public ContactController(IMailService mailService)
		{
			_mailService = mailService;
		}
		
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		//[ActionName("SendMail")]
		//public bool Index(MailData mailData) => _mailService.SendMail(mailData);
		public IActionResult Index(MailData mailData)
		{

			if (_mailService.SendMail(mailData))
			{
				TempData["success"] = "Thankyou for reaching us we will get back to you!!";
				return RedirectToAction(nameof(Index));
			}
			else
			{
				TempData["error"] = "Please wait and try after sometime!!";
				return RedirectToAction(nameof(Index));
			}
		}
	}
}
