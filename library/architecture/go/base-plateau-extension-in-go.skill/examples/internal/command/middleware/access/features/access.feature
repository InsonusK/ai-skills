@type/tech-check
Feature: Access check by the command's mark
  As the owner of a command only some callers may run
  I want a command marked restricted to run only for a caller with its role
  So that every API serving the command gets the same check, written once

  @category/happy
  Scenario: A restricted command runs for a caller with its role
    Given the caller "alice" with the roles "planner"
    When a command restricted to "planner" goes through the access check
    Then the handler should have run

  @category/negative
  Scenario: A restricted command without a caller is rejected as unauthenticated
    Given no caller
    When a command restricted to "planner" goes through the access check
    Then the call should be rejected as unauthenticated with "unauthenticated: Echo needs a caller"
    And the handler should not have run

  @category/negative
  Scenario: A caller without the role is rejected as permission denied
    Given the caller "bob" with the roles ""
    When a command restricted to "planner" goes through the access check
    Then the call should be rejected as permission denied with "permission denied: Echo needs the role planner"
    And the handler should not have run

  @category/happy
  Scenario: A command without the mark runs without a caller
    Given no caller
    When a command without a mark goes through the access check
    Then the handler should have run
