@type/tech-check
Feature: Retry of a call the API did not answer, in Python
  As a service over a client of another service's API
  I want a call of an operation safe to repeat sent again while the API does not answer
  So that a short outage of the other service does not fail my caller, and nothing is done twice

  Background:
    Given the retry allows 3 attempts with a backoff of 1ms

  # asyncio carries no deadline to check before a wait: the caller's
  # cancellation ends the wait instead, and passes through as it is.
  @category/boundary
  Scenario: The caller's cancellation ends the wait as it is
    Given the retry allows 3 attempts with a backoff of 1s
    And a retryable call whose attempts end "unavailable|ok"
    When the call is run through the retry, cancelled after 100ms
    Then the call should be cancelled after 1 attempts
