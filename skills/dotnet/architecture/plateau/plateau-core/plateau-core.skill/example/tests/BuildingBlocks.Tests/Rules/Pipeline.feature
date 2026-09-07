Feature: MediatR pipeline behaviors

  Scenario: An invalid request is short-circuited before the handler runs
    Given a pipeline with ValidationBehavior and a validator that rejects the request
    When the request goes through the pipeline
    Then the handler is not invoked
    And the result is invalid

  Scenario: An unhandled exception is converted to a safe error result
    Given a pipeline with ExceptionHandlingBehavior
    When an inner step throws an exception
    Then the result is an error with message "An unexpected error occurred. Please try again later."
