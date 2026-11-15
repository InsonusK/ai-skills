@type/tech-check
Feature: The forms of DATABASE_DSN in dotnet
  As the operator of the service
  I want a DSN in the form of Npgsql read as it is, and one it cannot use refused
  So that a connection string that cannot work stops the start

  @category/happy
  Scenario: The DSN is read as it is
    Given the environment variable "DATABASE_DSN" is "Host=db;Database=todo;Username=todo;Password=secret"
    When the settings are loaded
    Then the DSN should be "Host=db;Database=todo;Username=todo;Password=secret"

  @category/negative
  Scenario: A DSN that names no host is refused
    Given the environment variable "DATABASE_DSN" is "Database=todo"
    When the settings are loaded
    Then loading should fail mentioning "names no Host"
