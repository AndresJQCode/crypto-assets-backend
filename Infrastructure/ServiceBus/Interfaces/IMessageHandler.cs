using Azure.Messaging.ServiceBus;

namespace Infrastructure.ServiceBus.Interfaces
{
    public interface IMessageHandler
    {
        string TopicName { get; }
        string SubscriptionName { get; }
        Task HandleMessageAsync(ServiceBusReceivedMessage message);
    }
}
