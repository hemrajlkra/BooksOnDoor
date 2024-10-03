using BooksOnDoor.DataAccess.Repository;
using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Utility.Service;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BooksOnDoorWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class ContactController : Controller
	{
		private readonly IMailService _mailService;
		private readonly IConfiguration _configuration;

		public ContactController(IMailService mailService,IConfiguration configuration)
		{
			_mailService = mailService;
			_configuration = configuration;
		}
		
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		//[ActionName("SendMail")]
		//public bool Index(MailData mailData) => _mailService.SendMail(mailData);
		public async Task<IActionResult> Index(MailData mailData)
		{
			string secretKey = _configuration["ReCaptchaSetting:SecretKey"];
			bool success = await ReCaptchaService.verifyReCaptchaV2(mailData.ReCaptchaToken, secretKey);
			if (success==true)
			{
				_mailService.SendMail(mailData);
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
