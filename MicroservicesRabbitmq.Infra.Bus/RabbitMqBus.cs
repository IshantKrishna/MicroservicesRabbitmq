using System.Text;
using MediatR;
using MicroservicesRabbitmq.Domain.Core.Bus;
using MicroservicesRabbitmq.Domain.Core.Commands;
using MicroservicesRabbitmq.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroservicesRabbitmq.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus
    {
        private readonly IMediator _mediator;
        private Dictionary<string, HashSet<Type>> _handler;
        private readonly List<Type> _eventTypes;

        public RabbitMqBus(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _handler = new Dictionary<string, HashSet<Type>>();
            _eventTypes = new List<Type>();

        }
        public Task SendCommand<T>(T command) where T : Command
        {
           return _mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var eventName = @event.GetType().Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", eventName, null, body);
        }

        public void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handler.ContainsKey(eventName)) return;

            if (_handler[eventName].Any(h => h == handlerType))
                throw new InvalidOperationException(
                    $"Event name {eventName}, handler type {handlerType} is already subscribed.");
            _handler[eventName].Add(handlerType);
            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(typeof(T).Namespace, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(typeof(T).Namespace, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.Span);
            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Suppressing the error here
            }

        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handler.ContainsKey(eventName))
            {
                foreach (var subscriber in _handler[eventName])
                {
                    var handler = Activator.CreateInstance(subscriber);
                    if(handler == null) continue;
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType == null) continue;
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    if(@event == null) continue; 
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await ((Task)concreteType.GetMethod("Handle")?.Invoke(handler, new object[] { @event })!)!;
                }
            }
        }
    }
}
