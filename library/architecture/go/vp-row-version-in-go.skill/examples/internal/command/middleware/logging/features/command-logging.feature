@type/tech-check
Feature: Command logging middleware
  As an operator of the application
  I want every command logged when it is called and when it ends, with its parameters
  So that every command run is visible without reading each handler

  @category/happy
  Scenario: A handled command is logged when called, with its parameters, and when handled, with its result
    Given a handler of PlanItems that returns "ok", wrapped with logging
    When the wrapped handler handles PlanItems for "buy milk"
    Then the records should be:
      | level | msg             | command   | payload              | failed |
      | INFO  | command called  | PlanItems | {"title":"buy milk"} |        |
      | INFO  | command handled | PlanItems | ok                   | false  |

  @category/error
  Scenario: A failed command is logged at warn level with its error and its parameters
    Given a handler of PlanItems that fails with "boom", wrapped with logging
    When the wrapped handler handles PlanItems for "buy milk"
    Then the wrapped handler should fail with "boom"
    And the records should be:
      | level | msg            | command   | payload              | failed | error |
      | INFO  | command called | PlanItems | {"title":"buy milk"} |        |       |
      | WARN  | command failed | PlanItems | {"title":"buy milk"} | true   | boom  |

  @category/error
  Scenario: A failed command with a kind logs the message it answered and, apart, its cause
    Given a handler of PlanItems that fails as unavailable with "the lists are unavailable" caused by "dial tcp 10.0.0.5:5432: connection refused", wrapped with logging
    When the wrapped handler handles PlanItems for "buy milk"
    Then the wrapped handler should fail with "the lists are unavailable"
    And the records should be:
      | level | msg            | command   | error                     | cause                                      | exception |
      | INFO  | command called | PlanItems |                           |                                            |           |
      | WARN  | command failed | PlanItems | the lists are unavailable | dial tcp 10.0.0.5:5432: connection refused |           |
