@type/infrastructure/db/postgres
Feature: A database that does not answer
  As the service over the store and the orchestrator of the deployment
  I want to be told the database does not answer, apart from every other failure
  So that a caller may retry and traffic is held back

  @category/error
  Scenario: An insert is told the database is unavailable
    Given a database that does not answer
    When I insert "buy milk" into the list "home"
    Then the store should answer the database is unavailable

  @category/error
  Scenario: A select is told the database is unavailable
    Given a database that does not answer
    When I select the list "home"
    Then the store should answer the database is unavailable

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
