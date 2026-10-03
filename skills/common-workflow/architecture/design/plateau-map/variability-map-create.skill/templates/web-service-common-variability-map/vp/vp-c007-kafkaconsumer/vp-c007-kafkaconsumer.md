# VP-C007 KafkaConsumer

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C007 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Consuming events from Kafka. Every rule of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c006-kafkaproducer/vp-c006-kafkaproducer|VP-C006 KafkaProducer]] applies; the consumer group is the unit of parallelism, one partition per consumer at a time.
