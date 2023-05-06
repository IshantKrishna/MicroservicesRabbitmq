using MicroservicesRabbitmq.Domain.Core.Bus;
using MicroservicesRabbitmq.Infra.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace MicroservicesRabbitmq.Infra.IoC
{
    public class DependencyContainer
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            //Domain Bus
            serviceCollection.AddTransient<IEventBus, RabbitMqBus>();
        }

    }
}
