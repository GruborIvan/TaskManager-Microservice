using TaskManager.Domain.Models;

namespace TaskManager.Domain.IntegrationEvents
{
    public class StoreCommentSucceededEvent : IntegrationEvent
    {
        public StoreCommentSucceededEvent(Comment comment) : base()
        {
            Comment = comment;
        }

        public Comment Comment { get; }
    }
}
