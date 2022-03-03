namespace Onguard.TimeTracker.DAL
{
    public class ProjectModel
    {
        public string Name { get; }

        public string Url { get; }

        public ProjectModel(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}