using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Models;

namespace TaskManager.Infrastructure.Modules
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpdateTask, TaskDbo>()
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.InitiatedBy)); 

            CreateMap<Guid, CommentDbo>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src));

            CreateMap<Comment, CommentDbo>()
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy))
                .ReverseMap()
                .ConvertUsing(src => new Comment(
                        src.CommentId,
                        src.TaskId,
                        src.Text,
                        src.CreatedById,
                        src.CreatedDate
                    ));

            CreateMap<StoreComment, CommentDbo>()
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.InitiatedBy));

            CreateMap<Relation, TaskRelationDbo>()
                .ForMember(dest => dest.RelationId, opt => opt.MapFrom(src => src.RelationId))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ReverseMap();

            CreateMap<SaveTask, TaskDbo>()
                .ForMember(dest => dest.AssignedToEntityId, opt => opt.MapFrom(src => src.Assignment.AssignedToEntityId))
                .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.Assignment.Type))
                .ForMember(dest => dest.TaskRelations, opt => opt.MapFrom(src => src.Relations))
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.InitiatedBy))
                .ReverseMap()
                .ForMember(dest => dest.Assignment, opt => opt.MapFrom(src => new Assignment(src.AssignedToEntityId, src.AssignmentType, src.TaskId)));

            CreateMap<TaskDbo, Task>()
                .ConstructUsing(src => new Task(
                    src.TaskId,
                    src.TaskType,
                    new HttpCallback(src.Callback != null ? new Uri(src.Callback): null),
                    src.FourEyeSubjectId,
                    src.Subject,
                    new Source(
                        src.SourceId,
                        src.SourceName),
                    src.Comments == null
                        ? new List<Comment>()
                        : src.Comments.Select(c =>
                        new Comment(
                            c.CommentId,
                            c.TaskId,
                            c.Text,
                            c.CreatedById,
                            c.CreatedDate)),
                    src.Status,
                    src.Data,
                    new Assignment(
                        src.AssignedToEntityId,
                        src.AssignmentType,
                        src.TaskId),
                    src.TaskRelations == null
                        ? new List<Relation>()
                        : src.TaskRelations.Select(tr =>
                            new Relation(
                                tr.RelationId,
                                tr.TaskId,
                                tr.EntityId,
                                tr.EntityType,
                                tr.IsMain)),
                    src.CreatedById,
                    src.CreatedDate,
                    src.Change,
                    src.FinalState,
                    src.ChangedBy,
                    src.ChangedDate))
                .ReverseMap()
                .ForMember(dest => dest.AssignedToEntityId, opt => opt.MapFrom(src => src.Assignment.AssignedToEntityId))
                .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.Assignment.Type))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.TaskRelations, opt => opt.MapFrom(src => src.Relations))
                .ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => src.Source.SourceId))
                .ForMember(dest => dest.FinalState, opt => opt.MapFrom(src => src.IsFinal))
                .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.Source.SourceName))
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.Callback, opt => opt.MapFrom(src => src.Callback.Parameters));
        }
    }
}
