# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/infrastructure/db/postgres
Feature: Readiness of the database
  As the orchestrator of the deployment
  I want a check that the database answers
  So that traffic is held back during an outage

  @category/error
  Scenario: The readiness check fails while the database does not answer
    Given a database that does not answer
    When the readiness check runs
    Then the readiness check should be "unhealthy"

  @category/happy
  Scenario: The readiness check passes once the database answers
    Given a migrated database
    When the readiness check runs
    Then the readiness check should be "healthy"
