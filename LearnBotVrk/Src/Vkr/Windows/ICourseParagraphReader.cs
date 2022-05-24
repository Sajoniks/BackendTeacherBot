namespace LearnBotVrk.Vkr.Windows
{
    public interface ICourseParagraphReader
    {
        public bool NextPage();
        public bool PrevPage();

        public bool HasNext { get; }
        public bool HasPrevious { get; }
        
        public int CurrentPage { get; }
        public string CurrentPageContent { get; }
        public int PagesCount { get; }
    }
}