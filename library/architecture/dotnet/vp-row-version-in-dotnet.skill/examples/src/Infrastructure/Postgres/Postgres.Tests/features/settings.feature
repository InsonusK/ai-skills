# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/tech-check
Feature: Database settings
  As the operator of the service
  I want the database's settings read from the environment
  So that a wrong value stops the start with the variable named, and no password reaches a log

  @category/happy
  Scenario: The DSN is read, and the migration stays with the job by default
    Given a usable DATABASE_DSN with the password "secret"
    When the settings are loaded
    Then the application should migrate on start: "false"
    And the settings printed should not mention "secret"

  @category/happy
  Scenario: The migration can be moved to the start
    Given a usable DATABASE_DSN with the password "secret"
    And the environment variable "MIGRATE_ON_START" is "true"
    When the settings are loaded
    Then the application should migrate on start: "true"

  @category/negative
  Scenario: The DSN is required
    When the settings are loaded
    Then loading should fail mentioning "DATABASE_DSN"

  @category/negative
  Scenario: An unusable DSN is refused without being repeated
    Given an unusable DATABASE_DSN holding "secret"
    When the settings are loaded
    Then loading should fail mentioning "DATABASE_DSN"
    And the failure should not mention "secret"

  @category/negative
  Scenario: An unknown switch is refused
    Given a usable DATABASE_DSN with the password "secret"
    And the environment variable "MIGRATE_ON_START" is "maybe"
    When the settings are loaded
    Then loading should fail mentioning "MIGRATE_ON_START"
