namespace TaskManager.Domain.Models
{
    public class Source
    {
        public string SourceId { get; }
        public string SourceName { get; }

        public Source(
            string sourceId,
            string sourceName)
        {
            SourceId = sourceId;
            SourceName = sourceName;
        }
    }
}
