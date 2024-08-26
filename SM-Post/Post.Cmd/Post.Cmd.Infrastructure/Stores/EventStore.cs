using Cqrs.Domain.Exceptions;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;

        public EventStore(IEventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
        }

        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventSteam = await _eventStoreRepository.FindByAggregateId(aggregateId);
            if(eventSteam == null || !eventSteam.Any())
            {
                throw new AggregateNotFoundException("Incorrect post ID provided!");
            }
            return eventSteam.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
        }

        public async Task SaveEventAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventSteam = await _eventStoreRepository.FindByAggregateId(aggregateId);
            if (expectedVersion != -1 && eventSteam[^1].Version != expectedVersion)
                throw new CQRS.Core.Exceptions.ConcurrencyException();

            var version = expectedVersion;
            foreach(var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventStoreRepository.SaveAsync(eventModel);
            }
        }
    }
}
