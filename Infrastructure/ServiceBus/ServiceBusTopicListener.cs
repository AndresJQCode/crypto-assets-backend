using Azure.Messaging.ServiceBus;
using Infrastructure.ServiceBus.Interfaces;

namespace Infrastructure.ServiceBus;

public class ServiceBusTopicListener : IDisposable
{
    private readonly string _serviceBusConnectionString;
    private readonly string _topicName;
    private readonly string _subscriptionName;
    private ServiceBusClient _client = null!;
    private ServiceBusProcessor _processor = null!;
    private readonly IMessageHandler _messageHandler;

    public ServiceBusTopicListener(string serviceBusConnectionString, IMessageHandler messageHandler)
    {
        _serviceBusConnectionString = serviceBusConnectionString ?? throw new ArgumentNullException(nameof(serviceBusConnectionString));
        _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        _topicName = messageHandler.TopicName ?? throw new ArgumentNullException(nameof(messageHandler));
        _subscriptionName = messageHandler.SubscriptionName ?? throw new ArgumentNullException(nameof(messageHandler));

        InitializeProcessor();
    }

    private void InitializeProcessor()
    {
        _client = new ServiceBusClient(_serviceBusConnectionString);
        _processor = _client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
    }

    public void StartProcessing()
    {
        _processor.StartProcessingAsync();
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            await _messageHandler.HandleMessageAsync(args.Message);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el mensaje: {ex.Message}");
            await args.DeadLetterMessageAsync(args.Message, "ProcesamientoFallido", ex.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Message handler encountered an exception: {args.Exception}.");
        return Task.CompletedTask;
    }

    public async Task StopProcessingAsync()
    {
        await _processor.StopProcessingAsync();
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _processor?.DisposeAsync().AsTask().Wait();
            _client?.DisposeAsync().AsTask().Wait();
        }
    }
}
