# VP-C009 RabbitMqConsumer

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C009 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Consuming messages from RabbitMQ. Every rule of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c006-kafkaproducer/vp-c006-kafkaproducer|VP-C006 KafkaProducer]] applies; manual acknowledgement, and a dead-letter exchange receives messages the handler rejects as non-retryable.
