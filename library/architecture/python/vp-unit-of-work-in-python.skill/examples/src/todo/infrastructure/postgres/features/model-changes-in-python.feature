@type/infrastructure/db/postgres
Feature: The rows and their migrations
  As a developer who changes a row
  I want a row that no migration matches to fail
  So that the schema and the code do not drift

  @category/happy
  Scenario: Every row matches the schema of the migrations
    Given a migrated database
    Then every row of the application should match its table

  @category/negative
  Scenario: A table that differs from its row fails the check
    Given a migrated database
    And another connection drops the column "done" of "list_items"
    Then the check of the rows should fail
