using LearnBotServer.API;
using LearnBotServer.Model;
using LearnBotVrk.Vkr.API;
using Microsoft.AspNetCore.Mvc;

namespace LearnBotServer.Controllers;

[Route("api/courses")]
public class CourseController : Controller
{
    private readonly UserContext db = new UserContext();
    
    [HttpPost]
    [Route("give")]
    public IActionResult GiveCourse(long userId, string courseId)
    {
        var user = db.Users.Find(userId);
        if (user != null)
        {
            var userCourse = new UserCourse()
            {
                CourseId = courseId,
                User = user
            };
            db.UserCourses.Add(userCourse);
            db.SaveChanges();  
        }

        return Ok();
    }

    [HttpGet]
    [Route("{userId}/{courseId}")]
    public IActionResult GetCourse(long userId, string courseId)
    {
        var user = db.Users.Find(userId);
        if (user != null)
        {
            var progress = db.Progresses
                .Where(p => p.UserId == user.Id && p.CourseId == courseId);  

            var course = Resources.GetCourse(courseId);
            var chapters = course.GetCourseChapters();
            
            var chaptersDynamic = chapters
                .Select(c => new
                {
                    Id = c.Id,
                    Title = c.Title,
                    Paragraphs = c.Paragraphs
                        .Select(p => new
                        {
                            Id = p.Key,
                            Title = p.Value.Title,
                            Completed = progress.Any(prg => prg.ParagraphId == p.Key && prg.ChapterId == c.Id)
                        })
                });

            var courseDynamic = new
            {
                Title = course.Title,
                Id = course.Id,
                Chapters = chaptersDynamic
            };
            return Json(Response<dynamic>.OkResponse(courseDynamic));
        }

        return Ok();
    }

    [HttpGet]
    [Route("{userId}/{courseId}/chapter/{chapterId}/page/{page}")]
    public IActionResult GetParagraphText(long userId, string courseId, int chapterId, string page)
    {
        var user = db.Users.Find(userId);
        if (user != null)
        {
            var course = Resources.GetCourse(courseId);
            var chapter = course.GetCourseChapter(chapterId);
            var par = chapter.GetParagraphText(page);

            return Json(Response<string>.OkResponse(par));
        }

        return Json(Response<string>.FailResponse());
    }
}