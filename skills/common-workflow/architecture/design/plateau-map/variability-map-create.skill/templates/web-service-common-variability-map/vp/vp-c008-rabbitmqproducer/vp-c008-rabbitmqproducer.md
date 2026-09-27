# VP-C008 RabbitMqProducer

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C008 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Publishing messages to RabbitMQ. Every rule of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c006-kafkaproducer/vp-c006-kafkaproducer|VP-C006 KafkaProducer]] applies; publisher confirms are on, so a publish counts as done only when the broker confirmed it.
