using LearnBotServer.API;
using LearnBotServer.Model;
using LearnBotVrk.Vkr.API;
using Microsoft.AspNetCore.Mvc;
namespace LearnBotServer.Controllers;

[Route("api/users")]
public class UserController : Controller
{
    [HttpPost]
    [Route("register")]
    public IActionResult Register(
        [Bind(Prefix = "user_id")] long id, 
        [Bind(Prefix = "first_name")] string firstName,
        [Bind(Prefix = "last_name")] string lastName,
        [Bind(Prefix = "phone_number")] string phoneNumber
    )
    {
        try
        {
            using (var ctx = new UserContext())
            {
                User user = new User()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Id = id
                };

                ctx.Users.Add(user);
                ctx.SaveChanges();
                return Json(Response<bool>.OkResponse(true));
            }
        }
        catch (Exception e)
        {
            
        }

        return Json(Response<bool>.FailResponse());
    }

    [HttpGet]
    [Route("{userId}")]
    public IActionResult IsRegistered(long userId)
    {
        using (var ctx = new UserContext())
        {
            try
            {
                var user = ctx.Users.Find(userId);
                if (user != null)
                {
                    return Json(Response<bool>.OkResponse(true));
                }
            }
            catch (Exception e)
            {
                return Json(Response<bool>.FailResponse());
            }
        }

        return Json(Response<bool>.FailResponse());
    }

    [HttpGet]
    [Route("{userId}/courses")]
    public IActionResult GetCourses(long userId)
    {
        using (var ctx = new UserContext())
        {
            var user = ctx.Users.Find(userId);
            if (user != null)
            {
                var courses = ctx.UserCourses
                    .Where(c => c.User == user)
                    .Select(c => Resources.GetCourse(c.CourseId))
                    .ToList();

                return Json(Response<List<Course>>.OkResponse(courses));
            }
        }

        return Json(Response<bool>.FailResponse());
    }
}