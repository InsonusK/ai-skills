@type/tech-check
Feature: Decorators given as one ordered list
  As a developer attaching a variation point
  I want the steps around commands given as one ordered list that wraps every handler
  So that each step reaches every command and the order of the steps is decided in one place

  @category/happy
  Scenario: The decorators run in the order of the list, the first the outermost
    Given a handler that succeeds, decorated by:
      | decorator | does   |
      | outer     | passes |
      | inner     | passes |
    When the decorated handler handles the command "buy milk" as "PlanItems"
    Then the calls should be "outer in, inner in, handler buy milk, inner out, outer out"
    And the result should be "planned buy milk"

  @category/happy
  Scenario: A decorator sees the name and the value of the command
    Given a handler that succeeds, decorated by:
      | decorator | does   |
      | outer     | passes |
    When the decorated handler handles the command "buy milk" as "PlanItems"
    Then the decorator "outer" should have seen the command "buy milk" named "PlanItems"

  @category/error
  Scenario: A decorator that rejects the command stops it before the handler and the decorators inside
    Given a handler that succeeds, decorated by:
      | decorator | does    |
      | outer     | passes  |
      | guard     | rejects |
      | inner     | passes  |
    When the decorated handler handles the command "buy milk" as "PlanItems"
    Then the error should be "rejected by guard"
    And the calls should be "outer in, guard in, outer out"
