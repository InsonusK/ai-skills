@type/tech-check
Feature: Unit of work middleware, in Python
  As a command that writes in several places
  I want the outermost command to end the unit of work, by how its handler ended
  So that every write of the command is kept, or none is

  @category/error
  Scenario: A cancelled command rolls back, and the cancellation goes on
    Given the handler is cancelled
    When a command runs through the unit of work middleware
    Then the unit of work should have been ended with "rollback"
    And the cancellation should have reached the caller
