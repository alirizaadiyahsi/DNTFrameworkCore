﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DNTFrameworkCore.Dependency;
using DNTFrameworkCore.Domain;
using DNTFrameworkCore.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace DNTFrameworkCore.Eventing
{
    public interface IEventBus : IScopedDependency
    {
        Task<Result> TriggerAsync(IBusinessEvent businessEvent);
        Task TriggerAsync(IDomainEvent domainEvent);
    }

    internal sealed class EventBus : IEventBus
    {
        private const string MethodName = "Handle";
        private readonly IServiceProvider _provider;

        public EventBus(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<Result> TriggerAsync(IBusinessEvent businessEvent)
        {
            var eventType = businessEvent.GetType();
            var handlerType = typeof(IBusinessEventHandler<>).MakeGenericType(eventType);

            var method = handlerType.GetMethod(MethodName, new[] {eventType});
            if (method == null) throw new InvalidOperationException();

            var handlers = _provider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var result = await (Task<Result>) method.Invoke(handler, new object[] {businessEvent});

                if (result.Failed) return result;
            }

            return Result.Ok();
        }

        public Task TriggerAsync(IDomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            var method = handlerType.GetMethod(MethodName, new[] {eventType});
            if (method == null) throw new InvalidOperationException();

            var handlers = _provider.GetServices(handlerType);

            var tasks = handlers.Select(
                async handler => await (Task) method.Invoke(handler, new object[] {domainEvent}));

            return Task.WhenAll(tasks);
        }
    }
}