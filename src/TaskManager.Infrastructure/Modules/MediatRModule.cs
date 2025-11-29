using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;
using TaskManager.Domain.Commands;
using TaskManager.Domain.Validators;

namespace TaskManager.Infrastructure.Modules
{
    public class MediatRModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(SaveTaskHandler).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly)
                  .AsClosedTypesOf(typeof(IValidator<>));

            builder.RegisterGeneric(typeof(ValidationBehavior<,>))
                  .As(typeof(IPipelineBehavior<,>))
                  .InstancePerDependency();

            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IRequestHandler<,>));
            builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(INotificationHandler<>));

            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
    }
}
