using System.Reflection;
using Infrastructure.ServiceBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ServiceBus
{
    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMessageHandler CreateHandler(string handlerName)
        {
            var messageHandlerType = typeof(IMessageHandler);
            var handlerType = AppDomain.CurrentDomain.GetAssemblies()
                                     .SelectMany(a => a.GetTypes())
                                     .FirstOrDefault(t => !t.IsInterface && !t.IsAbstract &&
                                                         messageHandlerType.IsAssignableFrom(t) &&
                                                         t.Name.Equals(handlerName, StringComparison.OrdinalIgnoreCase));

            if (handlerType == null)
            {
                throw new InvalidOperationException($"Handler {handlerName} no existe.");
            }

            return (IMessageHandler)_serviceProvider.GetRequiredService(handlerType);
        }

        public IEnumerable<IMessageHandler> CreateHandlers()
        {
            var handlers = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                .Select(handlerType => _serviceProvider.GetService(handlerType))
                                .OfType<IMessageHandler>();

            return handlers;
        }
    }
}
