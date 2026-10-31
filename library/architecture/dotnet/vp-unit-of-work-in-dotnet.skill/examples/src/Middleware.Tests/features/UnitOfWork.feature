@type/tech-check
Feature: Unit of work middleware
  As a command that writes in several places
  I want the outermost command to end the unit of work, by how its handler ended
  So that every write of the command is kept, or none is

  @category/happy
  Scenario: A handled command commits
    When a command runs through the unit of work middleware
    Then the unit of work should have been asked for "commit"
    And the command should answer its result
    And the caller's cancellation should not reach the commit

  @category/error
  Scenario: A failed command rolls back and keeps its error
    Given the handler fails with "taken"
    When a command runs through the unit of work middleware
    Then the unit of work should have been asked for "rollback"
    And the command should fail with "taken"

  @category/edge
  Scenario: A nested command joins and ends nothing
    Given a unit of work already begun by the outer command
    When a command runs through the unit of work middleware
    Then the unit of work should have been asked for nothing
    And the command should answer its result

  @category/edge
  Scenario: A nested command that fails leaves the ending to the outer command
    Given a unit of work already begun by the outer command
    And the handler fails with "taken"
    When a command runs through the unit of work middleware
    Then the unit of work should have been asked for nothing
    And the command should fail with "taken"

  @category/error
  Scenario: A failed commit rolls back and keeps its error
    Given the commit fails
    When a command runs through the unit of work middleware
    Then the unit of work should have been asked for "commit|rollback"
    And the command should fail with "commit failed"
