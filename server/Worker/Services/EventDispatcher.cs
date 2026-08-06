using Application.Shared.Interfaces;
using Domain.Auth.Events;
using Domain.Shared.Events;
using System.Text.Json;

namespace Worker.Services;

public class EventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;

    private readonly Dictionary<string, Type> _eventTypes = new();

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        RegisterEventTypes();
    }

    private void RegisterEventTypes()
    {
        // Register all event types here
        _eventTypes["UserCreatedEvent"] = typeof(UserCreatedEvent);
    }

    /// <summary>
    /// Dispatches the event to all registered handlers.
    /// </summary>
    /// <param name="eventType">The type of the event to dispatch.</param>
    /// <param name="payload">The serialized event payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        if (!_eventTypes.TryGetValue(eventType, out var type))
        {
            _logger.LogWarning("Event type {EventType} not registered.", eventType);
            return;
        }

        var @event = JsonSerializer.Deserialize(payload, type) as DomainEvent;
        if (@event == null)
        {
            _logger.LogError("Failed to deserialize event data for {EventType}.", eventType);
            return;
        }

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);
        var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();

        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers found for event type {EventType}.", eventType);
            return;
        }

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                _logger.LogWarning("Handler for event type {EventType} is null.", eventType);
                continue;
            }
            var handlerName = handler.GetType().FullName ?? handler.GetType().Name;
            try
            {
                if (await idempotencyService.EventHasProcessed(@event.EventId, handlerName, cancellationToken))
                {
                    _logger.LogInformation("Event {EventType} with ID {EventId} has already been processed by handler {HandlerName}.", eventType, @event.EventId, handlerName);
                    continue;
                }

                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    await (Task)handleMethod.Invoke(handler, new object[] { @event, cancellationToken })!;
                    await idempotencyService.MarkAsProcessed(@event.EventId, handlerName, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}.", eventType, handler.GetType().Name);
                // Throwing causes the outbox processor to mark the event as a failure
                // Any event handler combination that succeeded will be skipped in retries
                throw;
            }
        }
    }
}
