using System.Text.Json;
using LastHour.BuildingBlocks.Infrastructure.Events;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background service that drains the outbox table: it loads a batch of pending messages,
/// rehydrates their payloads, publishes them to in-process handlers and marks them processed.
/// Failed messages are retried on subsequent cycles until the configured retry limit, at which
/// point they are abandoned with the last error preserved. Processing is at-least-once; exactly
/// once delivery is out of scope for the outbox processor itself.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private static readonly Action<ILogger, Exception?> ProcessorDisabled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(ProcessorDisabled)),
            "Outbox processor is disabled; outbox messages will not be dispatched.");

    private static readonly Action<ILogger, Exception?> CycleFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(CycleFailed)),
            "Outbox processing cycle failed.");

    private static readonly Action<ILogger, Guid, Exception?> MessageFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(3, nameof(MessageFailed)),
            "Failed to process outbox message {OutboxMessageId}.");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _options;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessor"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory used to resolve per-cycle dependencies.</param>
    /// <param name="options">The outbox processor options.</param>
    /// <param name="logger">The logger used to record processing diagnostics.</param>
    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Enabled)
        {
            ProcessorDisabled(_logger, null);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                CycleFailed(_logger, exception);
            }

            await Task.Delay(_options.Value.ProcessingInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private static async Task DispatchAsync(IMediator mediator, OutboxMessage message, CancellationToken cancellationToken)
    {
        Type type = Type.GetType(message.Type)
                    ?? throw new InvalidOperationException($"Cannot resolve outbox message type '{message.Type}'.");

        object payload = JsonSerializer.Deserialize(message.Content, type, OutboxJson.SerializerOptions)
                         ?? throw new InvalidOperationException($"Outbox message '{message.Id}' has no content.");

        object notification = NotificationMessageFactory.Create(payload);
        await mediator.Publish(notification, cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        LastHourDbContext dbContext = scope.ServiceProvider.GetRequiredService<LastHourDbContext>();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        OutboxOptions options = _options.Value;

        List<OutboxMessage> messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(options.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (OutboxMessage message in messages)
        {
            try
            {
                await DispatchAsync(mediator, message, cancellationToken).ConfigureAwait(false);
                message.MarkProcessed(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                MessageFailed(_logger, message.Id, exception);
                message.RecordFailure(exception.Message, DateTime.UtcNow, options.MaxRetryCount);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
