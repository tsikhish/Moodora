using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Moodora.Controllers;

public class MoodoraController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }
}