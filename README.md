# EDAExercise
**Event Driven Architecture Exercise** is about exploring the latest event driven patterns with a few bits of tech and a library. Key decisions where taken before hand to help focus the decisions to focus in on tech.

1. Utilise [Streaming instead of Messaging](https://risingwave.com/blog/differences-between-messaging-queues-and-streaming-a-deep-dive/) inorder to facilitate event sourcing patterns and horizontal scaling
   - **Messaging Queues**: Messaging queues are a form of middleware that handle messages (data packets) between applications. They ensure that messages sent from a producer service are properly received by a consumer service, even if the consumer is not ready to process them immediately.
   - **Streaming**: Streaming is the continuous transfer of data where data can be processed as it comes in. Streaming platforms allow for real-time data processing, allowing immediate insights and actions based on the incoming data.
2. Utilise [Azure Event Hubs](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about) and [Azure Service bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) to facilitate the above as well as patterns for throttling
3. Investigate [Kafka](https://kafka.apache.org/) and [Confluent Kafka](https://www.confluent.io/) as alternatives to achieving the same result
4. Utilise a library called [Masstransit.io](https://masstransit.io/) for quick intoduction to patterns on easily accesing consumer and producer patterns and a lot more
