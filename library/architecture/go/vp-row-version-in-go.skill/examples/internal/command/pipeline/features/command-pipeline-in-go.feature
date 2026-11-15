@type/tech-check
Feature: Middleware given as one ordered list, in Go
  As a developer attaching a variation point
  I want a middleware that passes on a value of the wrong type stopped at the call
  So that Go's untyped middleware value never reaches a handler as the wrong command

  @category/error
  Scenario: A middleware that passes on a value of another type fails the call instead of the handler
    Given a handler that succeeds, wrapped by the middleware:
      | middleware | does               |
      | swapper    | passes a number on |
    When the wrapped handler handles the command "buy milk" as "PlanItems"
    Then the error should be "pipeline: a middleware of PlanItems passed on a int, not the command"
    And the calls should be "swapper in, swapper out"
