@type/tech-check
Feature: Base log handler, in python
  As an operator of a service without the logging variation point
  I want a failure of the service written with its stack
  So that I can debug it from the log; Go's errors carry no stack, so only this stack writes it

  @category/error
  Scenario: A failure of the service is written with its stack
    When a record "command failed" is logged with an error "store is down" of no kind
    Then the line should have "error" = "store is down"
    And the line should have an "exception"
