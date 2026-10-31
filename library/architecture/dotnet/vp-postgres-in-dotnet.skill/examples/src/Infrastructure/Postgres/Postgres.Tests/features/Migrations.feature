@type/infrastructure/db/postgres
Feature: Migrations
  As a deployment
  I want the database brought up to the latest migration
  So that the service starts on the schema its code expects

  @category/happy
  Scenario: A new database gets every table
    Given an empty database
    When the migrations are applied
    Then the table "list_items" should exist with 0 rows

  @category/boundary
  Scenario: A database already up to date stays as it is
    Given a migrated database
    And the list "home" holds "buy milk"
    When the migrations are applied
    Then the table "list_items" should exist with 1 rows

  @category/negative
  Scenario: Every change of the model has its migration
    Then the model should have no change without a migration
