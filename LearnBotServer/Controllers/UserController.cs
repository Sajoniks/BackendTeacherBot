using LearnBotServer.Model;
using Microsoft.AspNetCore.Mvc;
namespace LearnBotServer.Controllers;

[Route("api/users")]
public class UserController : Controller
{
    [HttpGet]
    [Route("")]
    public IActionResult GetUsers()
    {
        return Json(new string[] { } );
    }

    [HttpPost]
    [Route("register")]
    public IActionResult Register()
    {
        return Json(new string[] { "hello", "world!" });
    }
}