@type/infrastructure/db/postgres
Feature: The model and its migrations
  As a developer who changes an entity or its mapping
  I want a change of the model with no migration to fail
  So that the schema and the code do not drift

  @category/negative
  Scenario: Every change of the model has its migration
    Then the model should have no change without a migration
