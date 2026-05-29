# name

observability

# description

Defines logging, metrics, tracing and monitoring conventions.

# content

## Logging

Use structured logging.

Every log entry should contain:

* correlation id
* module
* request context

## Metrics

Expose metrics for:

* request duration
* database operations
* background jobs
* failures

## Tracing

Use OpenTelemetry for distributed tracing.

## Health Checks

Provide health checks for:

* database
* cache
* external integrations

## Rules

Avoid logging sensitive data.
