Feature: ExceptionHandlingBehavior

  Scenario: Unhandled exception is converted to a safe error result
    Given a MediatR pipeline with ExceptionHandlingBehavior
    When an inner behavior throws an exception
    Then the result is an error with message "An unexpected error occurred. Please try again later."
