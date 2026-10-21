@type/tech-check
Feature: Command logging behavior
  As an operator of the application
  I want every command logged when it is called and when it ends, with its parameters
  So that every command run is visible without reading each handler

  @category/happy
  Scenario: A handled command is logged when called, with its parameters, and when handled, with its result
    Given a handler of PlanItems that returns "ok", wrapped with logging
    When the wrapped handler handles PlanItems for "buy milk"
    Then the records should be:
      | level       | msg             | command   | payload              | failed |
      | Information | command called  | PlanItems | {"title":"buy milk"} |        |
      | Information | command handled | PlanItems | ok                   | False  |

  @category/error
  Scenario: A failed command is logged at warning level with its error and its parameters
    Given a handler of PlanItems that fails with "boom", wrapped with logging
    When the wrapped handler handles PlanItems for "buy milk"
    Then the wrapped handler should fail with "boom"
    And the records should be:
      | level       | msg            | command   | payload              | failed | error |
      | Information | command called | PlanItems | {"title":"buy milk"} |        |       |
      | Warning     | command failed | PlanItems | {"title":"buy milk"} | True   | boom  |
