@type/tech-check
Feature: Unit of work middleware, in Go
  As a command that writes in several places
  I want the outermost command to end the unit of work, by how its handler ended
  So that every write of the command is kept, or none is

  @category/error
  Scenario: A handler that panics rolls back and panics on
    Given the handler panics
    When a command runs through the unit of work middleware
    Then the unit of work should have been ended with "rollback"
    And the panic should have reached the caller
