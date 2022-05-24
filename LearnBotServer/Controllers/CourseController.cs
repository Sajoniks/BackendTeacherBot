using LearnBotServer.API;
using LearnBotServer.Model;
using LearnBotVrk.Vkr.API;
using Microsoft.AspNetCore.Mvc;

namespace LearnBotServer.Controllers;

[Route("api/{userId}/courses")]
public class CourseController : Controller
{
    private readonly UserContext db;
    public CourseController(UserContext ctx)
    {
        db = ctx;
    }
    
    
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
    [Route("{courseId}")]
    public IActionResult GetCourse(long userId, string courseId)
    {
        var user = db.Users.Find(userId);
        if (user != null)
        {
            var progress = db.Progresses
                .Where(p => p.UserId == user.Id && p.CourseId == courseId);

            var quizes = db.CompletedQuises
                .Where(p => p.UserId == user.Id && p.CourseId == courseId);

            var course = Resources.GetCourse(courseId);
            var chapters = course.GetCourseChapters();
            
            var chaptersDynamic = chapters
                .Select(c => new
                {
                    Id = c.Id,
                    CourseId = course.Id,
                    Title = c.Title,
                    Completed = quizes.Any(q => q.ChapterId == c.Id && q.CourseId == courseId),
                    Paragraphs = c.Paragraphs
                        .Select(p => new
                        {
                            Id = p.Key,
                            CourseId = course.Id,
                            ChapterId = c.Id,
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
    [Route("{courseId}/chapter/{chapterId}/page/{page}")]
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

    [HttpPost]
    [Route("{courseId}/chapter/{chapterId}/completeQuiz")]
    public IActionResult AddQuiz(long userId, string courseId, int chapterId)
    {
        if (db.CompletedQuises.FirstOrDefault(q => q.UserId == userId) == null)
        {
            var progression = new UserQuiz()
            {
                ChapterId = chapterId,
                CourseId = courseId,
                UserId = userId
            };

            db.CompletedQuises.Add(progression);
            db.SaveChanges();
        }

        return Json(Response<bool>.OkResponse(true));
    }

    [HttpPost]
    [Route("{courseId}/chapter/{chapterId}/complete")]
    public IActionResult AddProgression(long userId, string courseId, int chapterId, string paragraph)
    {
        var user = db.Users.Find(userId);
        if (user != null)
        {
            var progression = new UserProgress()
            {
                ChapterId = chapterId,
                CourseId = courseId,
                ParagraphId = paragraph,
                UserId = userId
            };

            db.Progresses.Add(progression);
            db.SaveChanges();
        }

        return Json(Response<bool>.OkResponse(true));
    }

    [HttpGet]
    [Route("{courseId}/chapter/{chapterId}/quiz")]
    public IActionResult GetQuizForChapter(long userId, string courseId, int chapterId)
    {
        var course = Resources.GetCourse(courseId);
        var chapter = course.GetCourseChapter(chapterId);
        var quiz = chapter.GetChapterQuiz();

        return Json( Response<dynamic>.OkResponse(new
        {
            course_id = course.Id,
            chapter_id = chapter.Id,
            questions = quiz.Questions.Select(q => new
            {
                text = q.Text,
                paragraph_id = q.ParagraphId,
                title = chapter.Paragraphs[q.ParagraphId].Title,
                correct_option_num = q.CorrectOptionId,
                options = q.OptionStrings,
            })
        }));
    }
}