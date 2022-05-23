using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using LearnBotServer.API;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LearnBotVrk.Vkr.API
{
    public static class Resources
    {
        private static readonly string ResoursesPath;
        private static readonly Dictionary<string, string> CoursesMapping = new Dictionary<string, string>();
        private static readonly IDeserializer Deserializer;

        private static class Constants
        {
            public static readonly string Chapters = "chapters";
            public static readonly string Paragraphs = "paragraph";
            public static readonly string Resources = "resources";

            public static readonly string ParagraphFileExtension = "txt";
        }
        
        /// <summary>
        /// Get path to the courses folder
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        private static String GetCoursePath(Course course)
        {
            return Path.GetDirectoryName(CoursesMapping[course.Id]) ?? "";
        }

        /// <summary>
        /// Get path to the chapters of the course
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        private static String GetChaptersPath(Course course)
        {
            return Path.Combine(GetCoursePath(course), Constants.Chapters);
        }

        /// <summary>
        /// Get path to the course chapter
        /// </summary>
        /// <param name="chapter"></param>
        /// <returns></returns>
        private static String GetChapterPath(CourseChapter chapter)
        {
            return Path.Combine(GetChaptersPath(chapter.Course), chapter.Id.ToString());
        }

        /// <summary>
        /// Get path to chapter's paragraphs
        /// </summary>
        /// <param name="chapter"></param>
        /// <returns></returns>
        private static String GetParagraphsPath(CourseChapter chapter)
        {
            return Path.Combine(GetChapterPath(chapter), Constants.Paragraphs);
        }

        /// <summary>
        /// Get path to paragraph
        /// </summary>
        /// <param name="paragraph"></param>
        /// <returns></returns>
        private static String GetParagraphPath(CourseParagraph paragraph)
        {
            return GetParagraphPath(paragraph.Chapter, paragraph.Id);
        }

        private static String GetParagraphPath(CourseChapter chapter, String id)
        {
            return Path.Combine(GetParagraphsPath(chapter), $"{id}.{Constants.ParagraphFileExtension}");
        }

        private static String GetQuizPath(CourseChapter chapter)
        {
            return Path.Combine(GetChapterPath(chapter), "quiz.yaml");
        }

        static Resources()
        {
            ResoursesPath = Path.Combine(Environment.CurrentDirectory, "resources");

            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
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
            var chapterYml = Path.Combine(GetChaptersPath(course), chapter.ToString(), "info.yaml");
            
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
            var chaptersPath = GetChaptersPath(course);

            List<CourseChapter> outChapters = new List<CourseChapter>();
            var dirs = Directory.GetDirectories(chaptersPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs)
            {
                var chapterId = int.Parse(Path.GetFileName(dir));
                outChapters.Add(course.GetCourseChapter(chapterId));
            }

            return outChapters;
        }
        
        public static String GetParagraphText(this CourseParagraph paragraph)
        {
            var paragraphPath = GetParagraphPath(paragraph);
            return File.ReadAllText(paragraphPath);
        }

        public static string GetParagraphText(this CourseChapter chapter, string id)
        {
            var paragraphPath = GetParagraphPath(chapter, id);
            return File.ReadAllText(paragraphPath);
        }

        public static CourseQuiz GetChapterQuiz(this CourseChapter chapter)
        {
            var quizPath = GetQuizPath(chapter);
            
            var content = File.ReadAllText(quizPath);
            var quiz = Deserializer.Deserialize<CourseQuiz>(content);
            quiz.Chapter = chapter;
            foreach (var quizQuestion in quiz.Questions)
            {
                quizQuestion.Quiz = quiz;
            }
            
            return quiz;
        }
    }
}