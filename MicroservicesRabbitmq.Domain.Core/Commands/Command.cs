using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroservicesRabbitmq.Domain.Core.Events;

namespace MicroservicesRabbitmq.Domain.Core.Commands
{
    public abstract class Command: Message
    {
        public DateTime TimeStamp { get; protected set; }
        protected Command()
        {
            TimeStamp = DateTime.Now;
        }

    }
}
