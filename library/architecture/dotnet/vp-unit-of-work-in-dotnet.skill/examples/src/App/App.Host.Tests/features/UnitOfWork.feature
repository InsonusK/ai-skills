@type/infrastructure/db/postgres
Feature: Nested commands in one unit of work
  As a caller of a command that runs other commands
  I want all of them to succeed together or leave nothing
  So that a failure halfway needs no cleaning up

  Background:
    Given the commands over a migrated database

  @category/happy
  Scenario: Every nested command is kept
    When I import "buy milk|walk the dog" into the list "home"
    Then the list "home" should hold exactly "buy milk|walk the dog"

  @category/negative
  Scenario: A nested command that fails leaves none of the others
    Given the list "home" already holds "walk the dog"
    When I import "buy milk|walk the dog|call mum" into the list "home"
    Then the import should have failed as a conflict
    And the list "home" should hold exactly "walk the dog"

  @category/boundary
  Scenario: Every command has a unit of work of its own, however many run
    When 30 commands run one after another
    Then the database should hold 30 items
