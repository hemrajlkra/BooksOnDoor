using DI_ServiceLifetime.Models;
using DI_ServiceLifetime.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace DI_ServiceLifetime.Controllers
{
    public class HomeController : Controller
    {
        private readonly IScopedGuidService _scoped1;
        private readonly IScopedGuidService _scoped2;
        private readonly ITransientGuidService _transient1;
        private readonly ITransientGuidService _transient2;
        private readonly ISingletonGuidService _singleton1;
        private readonly ISingletonGuidService _singleton2;



        public HomeController(
            IScopedGuidService scoped1,
            IScopedGuidService scoped2,
            ITransientGuidService transient1,
            ITransientGuidService transient2,
            ISingletonGuidService singleton1,
            ISingletonGuidService singleton2)
        {
            _scoped1 = scoped1;
            _scoped2 = scoped2;
            _transient1 = transient1;
            _transient2 = transient2;
            _singleton1 = singleton1;
            _singleton2 = singleton2;
        }

        public IActionResult Index()
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Transient1: {_transient1.GetGuid()}\n");
            message.Append($"Transient2: {_transient2.GetGuid()}\n\n");
            message.Append($"Scoped1: {_scoped1.GetGuid()}\n");
            message.Append($"Scoped2: {_scoped2.GetGuid()}\n\n");
            message.Append($"Singleton1: {_singleton1.GetGuid()}\n");
            message.Append($"Singleton2: {_singleton2.GetGuid()}\n\n");
            return Ok(message.ToString());
        }

        
    }
}