@type/tech-check
Feature: Handler logging decorator
  As an operator of the application
  I want one log record per handled command
  So that every command run is visible without reading each handler

  @category/happy
  Scenario: A handled command is logged at info level
    Given a handler that succeeds, wrapped with logging as "PlanItems"
    When the wrapped handler handles a command
    Then one "INFO" record "command handled" should be logged for "PlanItems"

  @category/error
  Scenario: A failed command is logged at warn level with its error
    Given a handler that fails with "boom", wrapped with logging as "PlanItems"
    When the wrapped handler handles a command
    Then the wrapped handler should return the error "boom"
    And one "WARN" record "command failed" should be logged for "PlanItems"
