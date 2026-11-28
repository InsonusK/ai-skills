@type/tech-check
Feature: Retry of a call the API did not answer, in Go
  As a service over a client of another service's API
  I want a call of an operation safe to repeat sent again while the API does not answer
  So that a short outage of the other service does not fail my caller, and nothing is done twice

  Background:
    Given the retry allows 3 attempts with a backoff of 1ms

  @category/boundary
  Scenario: No wait starts that would end past the caller's deadline
    Given the retry allows 3 attempts with a backoff of 1s
    And a retryable call whose attempts end "unavailable|ok"
    When the call is run through the retry within 100ms
    Then the call should fail as unavailable after 1 attempts
