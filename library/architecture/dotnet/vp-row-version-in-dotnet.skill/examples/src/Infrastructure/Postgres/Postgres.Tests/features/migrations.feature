# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
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
    Given a migrated database with 1 rows in "list_items"
    When the migrations are applied
    Then the table "list_items" should exist with 1 rows
