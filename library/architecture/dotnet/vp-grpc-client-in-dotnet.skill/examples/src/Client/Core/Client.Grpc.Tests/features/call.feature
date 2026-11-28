@type/tech-check
Feature: Calling a gRPC API
  As a service over an API adapter
  I want every call bounded by its timeout and its outcome reported the same way
  So that I tell an API that is down from one that rejected the call, on every API

  @category/happy
  Scenario: A request is sent and the answer received
    Given the test API answers "hello"
    When I call Echo with the text "hi"
    Then the call should succeed with the text "hello"
    And the test API should have received Echo with the text "hi"

  @category/happy
  Scenario: The request id of the call being served is sent on
    Given the test API answers "hello"
    When I call Echo with the text "hi" in the request "req-42"
    Then the test API should have received the request id "req-42"

  @category/boundary
  Scenario: Outside a request no request id is sent
    Given the test API answers "hello"
    When I call Echo with the text "hi"
    Then the test API should have received no request id

  @category/negative
  Scenario: A failed call is reported with its status code and message
    Given the test API fails with ALREADY_EXISTS and the message "the name is taken"
    When I call Echo with the text "hi"
    Then the call should fail with the code ALREADY_EXISTS and the message "the name is taken"

  @category/error
  Scenario Outline: An API that does not answer is unavailable
    Given the test API <fails>
    When I call Echo with the text "hi"
    Then the call should fail as unavailable

    Examples:
      | fails                                                   |
      | fails with UNAVAILABLE and the message "down"           |
      | fails with DEADLINE_EXCEEDED and the message "too slow" |
      | takes longer than the timeout                           |
      | is not listening                                        |

  @category/negative
  Scenario: A method the adapter does not list is refused before it is sent
    Given the test API answers "hello"
    When I call the unlisted method "/test.v1.EchoService/Other"
    Then the call should fail mentioning "no method of the adapter's list"
    And the test API should have received 0 calls

  @category/happy
  Scenario: The client middleware sees the call and may send it again, each attempt sent and logged
    Given the test API answers "hello"
    And a client middleware that sends every call twice
    When I call Echo with the text "hi"
    Then the middleware should have seen the call "/test.v1.EchoService/Echo" of "test-api", retryable "true"
    And the test API should have received 2 calls
    And the records should be, in order, "grpc client call sent|grpc client call answered|grpc client call sent|grpc client call answered"
