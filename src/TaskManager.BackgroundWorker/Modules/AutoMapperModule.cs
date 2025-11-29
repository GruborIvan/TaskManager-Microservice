using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using AutoMapper;
using FiveDegrees.Messages.Task;
using Rebus.Pipeline;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Models;
using TaskManager.Domain.Models.Reporting;
using TaskManager.Infrastructure.Models;

namespace TaskManager.BackgroundWorker.Modules
{
    public class AutoMapperModule : Autofac.Module
    {
        private const string _userIdHeaderKey = "x-user-id";
        private const string _externalIdHeaderKey = "x-external-id";

        private readonly Assembly _profileAssemblies;

        public AutoMapperModule(Assembly profileAssemblies)
        {
            _profileAssemblies = profileAssemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(_profileAssemblies);

                cfg.CreateMap<CreateTaskMsg, SaveTask>()
                    .ConstructUsing((src, ctx) =>
                    new SaveTask(src.SourceId, src.TaskId ,src.Data, src.Callback, src.TaskType, src.Status,
                        new Assignment(src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId ?? Guid.Empty),
                        src.FourEyeSubjectId, GetIdentity(),
                        ctx.Mapper.Map<IEnumerable<Domain.Models.Relation>>(src.Relations),
                        src.SourceName, src.Subject, null));

                cfg.CreateMap<CreateTaskMsgV2, SaveTask>()
                    .ConstructUsing((src, ctx) =>
                    new SaveTask(src.SourceId, src.TaskId ,src.Data, src.Callback, src.TaskType, src.Status,
                        new Assignment(src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId ?? Guid.Empty),
                        src.FourEyeSubjectId, GetIdentity(),
                        ctx.Mapper.Map<IEnumerable<Domain.Models.Relation>>(src.Relations),
                        src.SourceName, src.Subject, null));

                cfg.CreateMap<CreateTaskMsgV3, SaveTask>()
                    .ConstructUsing((src, ctx) =>
                    new SaveTask(src.SourceId, src.TaskId, src.Data, src.Callback, src.TaskType.ToString(), src.Status,
                        new Assignment(src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId ?? Guid.Empty),
                        src.FourEyeSubjectId, GetIdentity(),
                        ctx.Mapper.Map<IEnumerable<Domain.Models.Relation>>(src.Relations),
                        src.SourceName, src.Subject, null));

                cfg.CreateMap<FiveDegrees.Messages.Task.Relation, Domain.Models.Relation>()
                    .ConstructUsing((src, ctx) => new Domain.Models.Relation(Guid.NewGuid(), default, src.EntityId, src.EntityType));

                cfg.CreateMap<UpdateTaskMsg, UpdateTask>()
                    .ConstructUsing(src => new UpdateTask(src.TaskId, src.Data, src.Status, GetIdentity(), src.FinalState));

                cfg.CreateMap<UpdateTaskStatusMsg, UpdateStatus>()
                    .ConstructUsing(src => new UpdateStatus(src.TaskId, src.Status, GetIdentity(), src.FinalStatus));

                cfg.CreateMap<UpdateTaskStatusMsgV2, UpdateStatus>()
                    .ConstructUsing(src => new UpdateStatus(src.TaskId, src.Status, GetIdentity(), src.FinalStatus));

                cfg.CreateMap<FinalizeTaskStatusMsg, FinalizeStatus>()
                    .ConstructUsing(src => new FinalizeStatus(src.TaskId, src.Status, GetIdentity(), src.FinalStatus));

                cfg.CreateMap<FinalizeTaskStatusMsgV2, FinalizeStatus>()
                    .ConstructUsing(src => new FinalizeStatus(src.TaskId, src.Status, GetIdentity(), src.FinalStatus));

                cfg.CreateMap<UpdateTaskDataMsg, UpdateData>()
                    .ConstructUsing(src => new UpdateData(src.TaskId, src.Data, GetIdentity()));

                cfg.CreateMap<UpdateTaskDataMsgV2, UpdateData>()
                    .ConstructUsing(src => new UpdateData(src.TaskId, src.Data, GetIdentity()));

                cfg.CreateMap<AssignTaskToEntityMsg, AssignTaskToEntity>()
                    .ConstructUsing((src, ctx) => new AssignTaskToEntity(src.TaskId, new Assignment(src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId), GetIdentity()));

                cfg.CreateMap<AssignTaskToEntityMsgV2, AssignTaskToEntity>()
                    .ConstructUsing((src, ctx) => new AssignTaskToEntity(src.TaskId, new Assignment(src.AssignedToEntityId, src.AssignmentType.ToString(), src.TaskId), GetIdentity()));

                cfg.CreateMap<UnassignTaskMsg, UnassignTask>()
                    .ConstructUsing(src => new UnassignTask(src.TaskId, GetIdentity()));

                cfg.CreateMap<UnassignTaskMsgV2, UnassignTask>()
                    .ConstructUsing(src => new UnassignTask(src.TaskId, GetIdentity()));

                cfg.CreateMap<RelateTaskToEntityMsg, RelateTaskToEntity>()
                    .ConstructUsing(src => new RelateTaskToEntity(src.EntityId.ToString(), src.EntityType, src.TaskId, GetIdentity()));

                cfg.CreateMap<RelateTaskToEntityMsgV2, RelateTaskToEntity>()
                    .ConstructUsing(src => new RelateTaskToEntity(src.EntityId, src.EntityType, src.TaskId, GetIdentity()));

                cfg.CreateMap<RelateTaskToEntityMsgV3, RelateTaskToEntity>()
                    .ConstructUsing(src => new RelateTaskToEntity(src.EntityId, src.EntityType, src.TaskId, GetIdentity()));

                cfg.CreateMap<StoreCommentMsg, StoreComment>()
                    .ConstructUsing(src => new StoreComment(src.TaskId, src.Text, GetIdentity(), src.CreatedDate));

                cfg.CreateMap<StoreCommentMsgV2, StoreComment>()
                    .ConstructUsing(src => new StoreComment(src.TaskId, src.Text, GetIdentity(), src.CreatedDate));

                cfg.CreateMap<UpdateTaskMsgV2, UpdateTaskV2>()
                    .ForCtorParam("initiatedBy", opt => opt.MapFrom(_ => GetIdentity()));

                cfg.CreateMap<ReportingTaskMsg, CreateReport>()
                    .ForCtorParam("initiatedBy", opt => opt.MapFrom(_ => GetIdentity()));

                cfg.CreateMap<TaskDbo, TaskReport>();
                cfg.CreateMap<TaskRelationDbo, TaskRelationReport>();
                cfg.CreateMap<CommentDbo, CommentReport>();
            });

            builder.RegisterInstance(config).As<IConfigurationProvider>().ExternallyOwned();
            builder.RegisterType<Mapper>().As<IMapper>();
        }

        private Guid GetIdentity()
        {
            var currentContext = MessageContext.Current;

            var createdBy = default(Guid);
            if (currentContext.Headers.TryGetValue(_userIdHeaderKey, out var userId))
            {
                createdBy = Guid.Parse(userId);
            }

            var externalId = default(Guid);
            if (currentContext.Headers.TryGetValue(_externalIdHeaderKey, out var externalIdValue))
            {
                externalId = Guid.Parse(externalIdValue);
            }

            createdBy = createdBy.Equals(Guid.Empty) ? externalId : createdBy;

            return createdBy;
        }
    }
}
