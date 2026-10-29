@type/tech-check
Feature: Retry of a call the API did not answer
  As a service over a client of another service's API
  I want a call of an operation safe to repeat sent again while the API does not answer
  So that a short outage of the other service does not fail my caller, and nothing is done twice

  Background:
    Given the retry allows 3 attempts with a backoff of 1ms

  @category/happy
  Scenario: A retryable call the API answers on the third attempt succeeds
    Given a retryable call whose attempts end "unavailable|unavailable|ok"
    When the call is run through the retry
    Then the call should succeed after 3 attempts
    And 2 "api client call retried" warnings should name the API "todo-api" and the operation "POST /v1/items/plan"

  @category/error
  Scenario: A retryable call the API never answers is unavailable after the last attempt
    Given a retryable call whose attempts end "unavailable|unavailable|unavailable|ok"
    When the call is run through the retry
    Then the call should fail as unavailable after 3 attempts

  @category/negative
  Scenario: A call of an operation not marked retryable is sent once
    Given a call not marked retryable whose attempts end "unavailable|ok"
    When the call is run through the retry
    Then the call should fail as unavailable after 1 attempts
    And no "api client call retried" warning should be logged

  @category/negative
  Scenario: A call the API answered with a status is not sent again
    Given a retryable call whose attempts end "status|ok"
    When the call is run through the retry
    Then the call should fail with a status after 1 attempts

  @category/boundary
  Scenario: No wait starts that would end past the caller's deadline
    Given the retry allows 3 attempts with a backoff of 1s
    And a retryable call whose attempts end "unavailable|ok"
    When the call is run through the retry within 100ms
    Then the call should fail as unavailable after 1 attempts

  @category/boundary
  Scenario: One attempt in all sends no repeat
    Given the retry allows 1 attempts with a backoff of 1ms
    And a retryable call whose attempts end "unavailable|ok"
    When the call is run through the retry
    Then the call should fail as unavailable after 1 attempts
