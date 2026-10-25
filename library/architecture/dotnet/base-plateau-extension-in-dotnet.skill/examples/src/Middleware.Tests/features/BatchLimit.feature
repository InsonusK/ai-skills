@type/tech-check
Feature: Batch limit by a value of the command
  As the operator of the service
  I want a command marked as a batch rejected when it holds more items than the limit
  So that one call cannot make the service plan without bound

  @category/boundary
  Scenario: A batch of exactly the limit runs
    Given the batch limit 3
    When a batch of 3 items goes through the batch limit
    Then the handler should have run

  @category/negative
  Scenario: A batch above the limit is rejected as resource exhausted before its handler
    Given the batch limit 3
    When a batch of 4 items goes through the batch limit
    Then the call should be rejected as resource exhausted with "Echo takes at most 3 items at once, got 4"
    And the handler should not have run

  @category/happy
  Scenario: A command without the mark is not limited
    Given the batch limit 3
    When a command without a mark goes through the batch limit
    Then the handler should have run
