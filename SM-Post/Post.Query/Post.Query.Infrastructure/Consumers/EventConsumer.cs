﻿using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Post.Query.Infrastructure.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly IEventHandler _eventHandler;

        public EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler)
        {
            _config = config.Value;
            _eventHandler = eventHandler;
        }

        public void Consume(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetKeyDeserializer(Deserializers.Utf8)
                .Build();
            consumer.Subscribe(topic);

            while (true)
            {
                var consumeResult = consumer.Consume();

                if (consumeResult?.Message == null) continue;

                var options = new JsonSerializerOptions {  Converters = {new EventJsonConverter()} };

                var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);
                var handledMethod = _eventHandler.GetType().GetMethod("On", new Type[] {@event.GetType()  });

                if (handledMethod == null)
                {
                    throw new ArgumentNullException(nameof(handledMethod), "Could not find event handler method!");
                }
                handledMethod.Invoke(consumer, new object[] { @event });
                consumer.Commit(consumeResult);
            }
        }
    }
}
