using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
    public interface IEventStore 
    {
        void Save(IEvent @event);
        IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion);
    }
}