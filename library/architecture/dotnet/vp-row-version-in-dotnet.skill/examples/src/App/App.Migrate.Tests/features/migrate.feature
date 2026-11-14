@type/infrastructure/db/postgres
Feature: The migration job
  As a deployment
  I want a job that applies the migrations and tells how it ended
  So that the service starts only on a migrated database

  @category/happy
  Scenario: The job applies the migrations and exits 0
    Given an empty database
    When the migration job runs
    Then the job should exit with 0
    And the job should log "migrations applied"
    And the table "list_items" should exist

  @category/boundary
  Scenario: A second run finds nothing to do and exits 0
    Given an empty database
    When the migration job runs
    And the migration job runs
    Then the job should exit with 0

  @category/negative
  Scenario: Without a DSN the job fails and names the variable
    When the migration job runs
    Then the job should exit with 1
    And the job should log "DATABASE_DSN is required"

  @category/error
  Scenario: A database that does not answer fails the job
    Given a database that does not answer
    When the migration job runs
    Then the job should exit with 1
    And the job should log "migration failed"
