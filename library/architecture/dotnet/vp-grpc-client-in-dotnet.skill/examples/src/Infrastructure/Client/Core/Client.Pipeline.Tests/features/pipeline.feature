@type/tech-check
Feature: Client middleware
  As a variation point that adds a step to every call of every API (the retry)
  I want the client middleware of the composition root around the attempts of each call
  So that I add the step once, without editing any client or adapter

  @category/happy
  Scenario: The middleware run around the attempt, the first the outermost
    Given the client middleware "outer|inner"
    When a call of "POST /v1/echo" is run
    Then the steps should be, in order, "outer before|inner before|attempt|inner after|outer after"

  @category/happy
  Scenario: A middleware that calls next twice sends two attempts
    Given the client middleware "twice"
    When a call of "POST /v1/echo" is run
    Then the steps should be, in order, "twice before|attempt|attempt|twice after"

  @category/boundary
  Scenario: With no middleware the call is one attempt
    When a call of "POST /v1/echo" is run
    Then the steps should be, in order, "attempt"
