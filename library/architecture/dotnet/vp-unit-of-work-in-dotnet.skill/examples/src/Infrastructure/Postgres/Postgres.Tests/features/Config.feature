@type/tech-check
Feature: Database settings
  As the operator of the service
  I want the database's settings read from the environment
  So that a wrong value stops the start with the variable named, and no password reaches a log

  @category/happy
  Scenario: The DSN is read, and the migration stays with the job by default
    Given the environment variable "DATABASE_DSN" is "Host=db;Database=todo;Username=todo;Password=secret"
    When the settings are loaded
    Then the DSN should be "Host=db;Database=todo;Username=todo;Password=secret"
    And the application should migrate on start: "false"
    And the settings printed should not mention "secret"

  @category/happy
  Scenario: The migration can be moved to the start
    Given the environment variable "DATABASE_DSN" is "Host=db"
    And the environment variable "MIGRATE_ON_START" is "true"
    When the settings are loaded
    Then the application should migrate on start: "true"

  @category/negative
  Scenario: The DSN is required
    When the settings are loaded
    Then loading should fail mentioning "DATABASE_DSN is required"

  @category/negative
  Scenario: A value that is no connection string is refused without being repeated
    Given the environment variable "DATABASE_DSN" is "postgres:secret@db"
    When the settings are loaded
    Then loading should fail mentioning "DATABASE_DSN"
    And the failure should not mention "secret"

  @category/negative
  Scenario: A DSN that names no host is refused
    Given the environment variable "DATABASE_DSN" is "Database=todo"
    When the settings are loaded
    Then loading should fail mentioning "names no Host"

  @category/negative
  Scenario: An unknown switch is refused
    Given the environment variable "DATABASE_DSN" is "Host=db"
    And the environment variable "MIGRATE_ON_START" is "maybe"
    When the settings are loaded
    Then loading should fail mentioning "MIGRATE_ON_START"
