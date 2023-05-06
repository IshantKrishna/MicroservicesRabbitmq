using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroservicesRabbitmq.Domain.Core.Events;

namespace MicroservicesRabbitmq.Domain.Core.Bus
{
    public interface IEventHandler<in TEvent> : IEventHandler
        where TEvent : Event
    {
    }

    public interface IEventHandler{ }
}
