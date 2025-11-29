using TaskManager.API.Models;
using TaskManager.Domain.Models;
using AutoMapper;
using TaskManager.Domain.Commands;
using Task = TaskManager.Domain.Models.Task;
using System;
using System.Collections.Generic;
using Relation = TaskManager.Domain.Models.Relation;

namespace TaskManager.API.Modules
{
    public class AutoMapperProfileApi : Profile
    {
        public AutoMapperProfileApi()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy));

            CreateMap<Relation, RelationDto>();

            CreateMap<(Task task, Guid userId), TaskDto>()
                .ForMember(dest => dest.Callback, opt => opt.MapFrom(src => src.task.Callback.Parameters))
                .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.task.Change))
                .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.task.ChangedBy))
                .ForMember(dest => dest.ChangedDate, opt => opt.MapFrom(src => src.task.ChangedDate))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.task.Comments))
                .ForMember(dest => dest.Relations, opt => opt.MapFrom(src => src.task.Relations))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.task.Status))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.task.CreatedBy))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.task.CreatedDate))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.task.Data))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.task.Subject))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.task.TaskId))
                .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => src.task.TaskType))
                .ForMember(dest => dest.AssignedToEntityId, opt => opt.MapFrom(src => src.task.Assignment.AssignedToEntityId))
                .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.task.Assignment.Type))
                .ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => src.task.Source.SourceId))
                .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.task.Source.SourceName))
                .ForMember(dest => dest.FinalState, opt => opt.MapFrom(src => src.task.IsFinal))
                .ForMember(dest => dest.SubjectUnder4Eye, opt => opt.MapFrom(src => src.task.FourEyeSubjectId == src.userId));

            CreateMap<SendCreateTaskMessageRequest, SaveTask>()
                .ConstructUsing((src, ctx) =>
                new SaveTask(
                    src.SourceId, src.TaskId, src.Data, src.Callback, src.TaskType.ToString(), src.Status,
                    new Assignment(
                        src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId ?? Guid.Empty),
                    src.FourEyeSubjectId, src.RequestorId,
                    ctx.Mapper.Map<IEnumerable<Relation>>(src.Relations),
                    src.SourceName, src.Subject, src.Comment));

            CreateMap<RelationDto, Relation>()
                .ConstructUsing((src) => new Relation(Guid.NewGuid(), default, src.EntityId, src.EntityType));
        }
    }
}
