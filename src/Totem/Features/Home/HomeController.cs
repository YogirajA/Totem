using System.Diagnostics;
using Totem.Models;
using Microsoft.AspNetCore.Mvc;

namespace Totem.Features.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //Redirect the main page to Contracts home
            return Redirect("Contracts/Index");
        }
    }
}
