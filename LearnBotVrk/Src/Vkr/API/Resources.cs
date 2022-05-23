using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LearnBotVrk.Vkr.API
{
    public static class Resources
    {
        private static readonly string ResoursesPath;
        private static readonly Dictionary<string, string> CoursesMapping = new Dictionary<string, string>();
        private static readonly IDeserializer Deserializer;

        static Resources()
        {
            ResoursesPath = Path.Combine(Environment.CurrentDirectory, "resources");

            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                // .IgnoreUnmatchedProperties()
                .Build();
            
            var dirs = Directory.EnumerateDirectories(ResoursesPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs)
            {
                var path = Path.Combine(dir, "info.yaml");
                var yml = File.ReadAllText(path);
                var info = Deserializer.Deserialize<Course>(yml);

                var folderName = Path.GetFileName( Path.GetDirectoryName(path) );
                CoursesMapping.Add(info.Id, path);
            }
        }

        public static Course GetCourse(string id)
        {
            return Deserializer.Deserialize<Course>(
                File.ReadAllText(CoursesMapping[id])    
            );
        }
        
        public static CourseChapter GetCourseChapter(this Course course, int chapter)
        {
            var coursePath = course.GetCoursePath();
            var chapterPath = Path.Combine(coursePath , "chapters", chapter.ToString());
            var chapterYml = Path.Combine(chapterPath, "info.yaml");

            var chap = Deserializer.Deserialize<CourseChapter>(File.ReadAllText(chapterYml));
            chap.Id = chapter;
            chap.Course = course;

            foreach (var courseParagraph in chap.Paragraphs)
            {
                courseParagraph.Value.Chapter = chap;
                courseParagraph.Value.Id = courseParagraph.Key;
            }
            
            return chap;
        }

        public static List<CourseChapter> GetCourseChapters(this Course course)
        {
            var coursePath = course.GetCoursePath();
            var chaptersPath = Path.Combine(coursePath, "chapters");

            List<CourseChapter> outChapters = new List<CourseChapter>();
            foreach (var dir in Directory.GetDirectories(chaptersPath, "*", SearchOption.TopDirectoryOnly))
            {
                outChapters.Add(course.GetCourseChapter( int.Parse( Path.GetFileName(dir) )));
            }

            return outChapters;
        }

        private static String GetCoursePath(this Course course)
        {
            return Path.GetDirectoryName(CoursesMapping[course.Id]);
        }
        
        public static String GetParagraphText(this CourseParagraph paragraph)
        {
            var coursePath = paragraph.Chapter.Course.GetCoursePath();
            var paragraphPath = Path.Combine(coursePath, "chapters", paragraph.Chapter.Id.ToString(), "paragraph",
                $"{paragraph.Id}.txt");
            return File.ReadAllText(paragraphPath);
        }

        public static CourseQuiz GetChapterQuiz(this CourseChapter chapter)
        {
            var coursePath = chapter.Course.GetCoursePath();
            
            var paragraphPath = Path.Combine(coursePath, "chapters", chapter.Id.ToString(), "paragraph", "quiz.yaml");
            var content = File.ReadAllText(paragraphPath);
            var quiz = Deserializer.Deserialize<CourseQuiz>(content);
            
            quiz.Chapter = chapter;
            foreach (var quizQuestion in quiz.Questions)
            {
                quizQuestion.Quiz = quiz;
            }
            
            return quiz;
        }

        public static CourseParagraph GetParagraph(this CourseQuiz.Question question)
        {
            return question.Quiz.Chapter.Paragraphs[question.ParagraphId];
        }
    }
}