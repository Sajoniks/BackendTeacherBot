using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace LearnBotVrk.Vkr.API
{
    public class CourseQuiz
    {
        public class Question
        {
            [JsonProperty("text")] public string Text { get; private set; }
            [JsonProperty("paragraph_id")] public string ParagraphId { get; private set; }
            [JsonProperty("title")] public string ParagraphTitle { get; private set; }
            [JsonProperty("options")] public string[] Options { get; private set; }
            [JsonProperty("correct_option_num")] public int CorrectOptionNum { get; private set; }

            public string CorrectOption => Options[CorrectOptionNum - 1];
        }

        [JsonProperty("course_id")] public string CourseId { get; private set; }
        [JsonProperty("chapter_id")] public int ChapterId { get; private set; }
        [JsonProperty("questions")] public Question[] Questions { get; private set; }
    }
}