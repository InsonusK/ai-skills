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
    When the decorated handler handles the command "buy milk"
    Then the calls should be "outer in, inner in, handler buy milk, inner out, outer out"
    And the result should be "planned buy milk"

  @category/error
  Scenario: A decorator that rejects the command stops it before the handler and the decorators inside
    Given a handler that succeeds, decorated by:
      | decorator | does    |
      | outer     | passes  |
      | guard     | rejects |
      | inner     | passes  |
    When the decorated handler handles the command "buy milk"
    Then the error should be "rejected by guard"
    And the calls should be "outer in, guard in, outer out"

  @category/negative
  Scenario: A type that is no open generic pipeline behavior is refused when it is listed
    When the type "String" is listed as a decorator
    Then listing should fail mentioning "String is not an open generic IPipelineBehavior<,>"
