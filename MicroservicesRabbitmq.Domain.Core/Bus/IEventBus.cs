using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroservicesRabbitmq.Domain.Core.Commands;
using MicroservicesRabbitmq.Domain.Core.Events;

namespace MicroservicesRabbitmq.Domain.Core.Bus
{
    internal interface IEventBus
    {
        Task SendCommand<T>() where T : Command;

        void Publish<T>(T @event) where T : Event;

        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;
    }
}
