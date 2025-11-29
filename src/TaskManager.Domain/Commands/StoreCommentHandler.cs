using System.Threading;
using System.Threading.Tasks;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using TaskManager.Domain.Validators;

namespace TaskManager.Domain.Commands
{
    public class StoreCommentHandler : ICommandHandler<StoreComment, Comment>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly StoreCommentValidator _validator;

        public StoreCommentHandler(
            ITaskRepository taskRepository,
            ICommentRepository commentRepository,
            StoreCommentValidator validator)
        {
            _taskRepository = taskRepository;
            _commentRepository = commentRepository;
            _validator = validator;
        }

        public async Task<Comment> Handle(StoreComment request, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(request);

            await _taskRepository.GetAsync(request.TaskId, cancellationToken);

            var addedComment = await _commentRepository.AddAsync(CreateComment(request), cancellationToken);

            await _commentRepository.SaveAsync(cancellationToken);

            return addedComment;
        }

        private Comment CreateComment(StoreComment request)
            => new Comment(request.TaskId, request.Text, request.InitiatedBy, request.CreatedDate);
    }
}
