using LearnBotServer.API;
using LearnBotServer.Model;
using LearnBotVrk.Vkr.API;
using Microsoft.AspNetCore.Mvc;
namespace LearnBotServer.Controllers;

[Route("api/{userId}")]
public class UserController : Controller
{
    private UserContext _db;
    public UserController(UserContext context)
    {
        _db = context;
    }
    
    [HttpPost]
    [Route("register")]
    public IActionResult Register(
        long userId, 
        [Bind(Prefix = "first_name")] string firstName,
        [Bind(Prefix = "last_name")] string lastName,
        [Bind(Prefix = "phone_number")] string phoneNumber
    )
    {
        User user = new User()
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Id = userId
        };

        _db.Users.Add(user);
        _db.SaveChanges();
        return Json(Response<bool>.OkResponse(true));
    }

    [HttpGet]
    [Route("")]
    public IActionResult IsRegistered(long userId)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            return Json(Response<bool>.OkResponse(true));
        }

        return Json(Response<bool>.FailResponse());
    }

    [HttpGet]
    [Route("courses")]
    public IActionResult GetCourses(long userId)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            var courses = _db.UserCourses
                .Where(c => c.User == user)
                .Select(c => Resources.GetCourse(c.CourseId))
                .ToList();

            return Json(Response<List<Course>>.OkResponse(courses));
        }

        return Json(Response<bool>.FailResponse());
    }

    [HttpGet]
    [Route("profile")]
    public IActionResult GetProfile(long userId)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            var courses = _db.UserCourses
                .Where(c => c.User == user)
                .Select(c => Resources.GetCourse(c.CourseId))
                .ToList();

            var quizes = _db.CompletedQuises
                .Where(q => q.UserId == userId);

            List<dynamic> items = new List<dynamic>();
            foreach (var course in courses)
            {
                int total = courses
                    .Select(c => c.GetCourseChapters())
                    .Sum(l => l.Sum(ch => ch.Paragraphs.Count));

                int quizesTotal = quizes
                    .Count(q => q.CourseId == course.Id);

                int done = _db.Progresses
                    .Count(p => p.CourseId == course.Id && p.UserId == userId);

                float progress = (float)(done + quizesTotal) / (total + course.GetCourseChapters().Count);
                
               items.Add(new
               {
                   course_id = course.Id,
                   title = course.Title,
                   progress = progress,
                   total_quiz_num = course.GetCourseChapters().Count,
                   completed_quiz_num = quizesTotal,
                   total_pages_num = total,
                   completed_pages_num = done
               });
            }

            return Json(Response<List<dynamic>>.OkResponse(items));
        }

        return Json(Response<bool>.FailResponse());
    }
}