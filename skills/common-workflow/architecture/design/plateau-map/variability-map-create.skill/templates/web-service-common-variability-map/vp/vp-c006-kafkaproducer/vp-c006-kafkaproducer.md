# VP-C006 KafkaProducer

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C006 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

Publishing events to Kafka. The messaging rules below apply to every broker VP (VP-C006–VP-C009); the four are independent, so a service may publish and consume over either broker in any combination.
- **CloudEvents 1.0 envelope** — every message is a CloudEvent (`id`, `source`, `type`, `time`, `datacontenttype`, `data`; trace context through the distributed-tracing extension). Kafka uses the CloudEvents Kafka protocol binding; RabbitMQ uses structured mode (`application/cloudevents+json`), which works over AMQP 0-9-1 and 1.0 alike.
- **Order only within a key** — the message key (Kafka partition key, RabbitMQ routing to one queue) is the only ordering unit.
- **Producer outcome** — by [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c004-httpoutbound/vp-c004-httpoutbound|VP-C004's outcome and retry rules]]: broker unavailable → `503`, message rejected (too large, invalid) → `400`. A retried publish keeps the same CloudEvents `id`, so a duplicate is recognisable.
- **Consumer semantics (baseline, detailed when the solutions are written)** — at-least-once delivery; handlers are idempotent and deduplicate by CloudEvents `id`; a handler's outcome is an HTTP status code: retryable per VP-C004's classification → redelivered, otherwise → dead-letter topic/queue; the message is acknowledged (offset committed) only after it is handled.
- **Shared messaging infrastructure** — envelope, serialization, and tracing are one stack-level building block that every broker VP depends on (a mandatory sub-feature), not repeated per VP.
