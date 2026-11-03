@type/infrastructure/db/postgres
Feature: The application over its database
  As a caller of the API
  I want what I stored to outlive the process, and the database's failures told in the API's answers
  So that the application behaves the same as with any store, and survives a restart

  @category/happy
  Scenario: An item outlives a restart
    Given a migrated database for the application
    And the application is running
    And "buy milk" was added to the list "home" over HTTP
    When the application restarts
    And I read the list "home" over HTTP
    Then the application should answer 200
    And the answer should mention "buy milk"

  @category/happy
  Scenario: The application can migrate on start
    Given an empty database for the application
    And the application migrates on start
    And the application is running
    When I add "buy milk" to the list "home" over HTTP
    Then the application should answer 201

  @category/negative
  Scenario: A title already in the list answers a conflict, and no error reaches the log
    Given a migrated database for the application
    And the application is running
    And "buy milk" was added to the list "home" over HTTP
    When I add "buy milk" to the list "home" over HTTP
    Then the application should answer 409
    And the log should have no error record

  @category/error
  Scenario: A database that does not answer is told as unavailable
    Given a database for the application that does not answer
    And the application is running
    When I read the list "home" over HTTP
    Then the application should answer 503
    And the log should not mention "An exception occurred"

  @category/error
  Scenario: A migration that fails stops the start
    Given a database for the application that does not answer
    And the application migrates on start
    When the application starts and ends
    Then the application should have exited with 1
    And the log should mention "startup failed"

  @category/negative
  Scenario: Without a DSN the application does not start
    When the application starts and ends
    Then the application should have exited with 1
    And the log should mention "DATABASE_DSN is required"
