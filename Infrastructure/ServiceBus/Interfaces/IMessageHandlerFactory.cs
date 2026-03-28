namespace Infrastructure.ServiceBus.Interfaces
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler CreateHandler(string handlerName);
        IEnumerable<IMessageHandler> CreateHandlers();
    }
}
