@type/tech-check
Feature: Handler logging decorator
  As an operator of the application
  I want every command logged when it is called and when it ends, with its parameters
  So that every command run is visible without reading each handler

  @category/happy
  Scenario: A handled command is logged when called, with its parameters, and when handled
    Given a handler that succeeds, wrapped with logging as "PlanItems"
    When the wrapped handler handles the command "buy milk"
    Then the records should be:
      | level | msg             | command   | payload  | failed |
      | INFO  | command called  | PlanItems | buy milk |        |
      | INFO  | command handled | PlanItems |          | false  |

  @category/error
  Scenario: A failed command is logged at warn level with its error and its parameters
    Given a handler that fails with "boom", wrapped with logging as "PlanItems"
    When the wrapped handler handles the command "buy milk"
    Then the wrapped handler should return the error "boom"
    And the records should be:
      | level | msg            | command   | payload  | failed |
      | INFO  | command called | PlanItems | buy milk |        |
      | WARN  | command failed | PlanItems | buy milk | true   |
