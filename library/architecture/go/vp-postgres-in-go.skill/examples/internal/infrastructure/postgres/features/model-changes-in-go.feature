@type/infrastructure/db/postgres
Feature: The rows and their migrations
  As a developer who changes a row
  I want a row that no migration matches to fail
  So that the schema and the code do not drift

  @category/negative
  Scenario: Every row matches the schema of the migrations
    Given a migrated database
    Then every row of the application should match its table
