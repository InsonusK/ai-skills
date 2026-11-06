@type/tech-check
Feature: Calling an HTTP API
  As a service over an API adapter
  I want every call sent as JSON, bounded by its timeout, and its outcome reported the same way
  So that I tell an API that is down from one that rejected the call, on every API

  @category/happy
  Scenario: A request is sent as JSON and the answer decoded
    Given the test API answers 200 with '{"text":"hello","extra":1}'
    When I call POST "/v1/echo" with the text "hi"
    Then the call should succeed with the text "hello"
    And the test API should have received POST "/v1/echo" with '{"text":"hi"}'

  @category/happy
  Scenario: The request id of the call being served is sent on
    Given the test API answers 200 with '{"text":"hello"}'
    When I call POST "/v1/echo" with the text "hi" in the request "req-42"
    Then the test API should have received the request id "req-42"

  @category/boundary
  Scenario: Outside a request no request id is sent
    Given the test API answers 200 with '{"text":"hello"}'
    When I call POST "/v1/echo" with the text "hi"
    Then the test API should have received no request id

  @category/negative
  Scenario: An error answer is reported with its status and its google.rpc.Status
    Given the test API answers 409 with '{"code":6,"message":"the name is taken","details":[]}'
    When I call POST "/v1/echo" with the text "hi"
    Then the call should fail with the status 409, the code 6, and the message "the name is taken"

  @category/negative
  Scenario: An error answer that is not a google.rpc.Status is reported with its status alone
    Given the test API answers 404 with '<html>not here</html>'
    When I call POST "/v1/echo" with the text "hi"
    Then the call should fail with the status 404, the code 0, and the message ""

  @category/error
  Scenario Outline: An API that does not answer is unavailable
    Given the test API <fails>
    When I call POST "/v1/echo" with the text "hi"
    Then the call should fail as unavailable

    Examples:
      | fails                                       |
      | answers 503 with '{"code":14,"message":"down"}' |
      | answers 502 with ''                         |
      | answers 504 with ''                         |
      | takes longer than the timeout               |
      | is not listening                            |

  @category/error
  Scenario: An answer that does not fit the response is neither unavailable nor a status
    Given the test API answers 200 with '{"text":7}'
    When I call POST "/v1/echo" with the text "hi"
    Then the call should fail mentioning "does not match the contract"

  @category/negative
  Scenario: An operation the adapter does not list is refused before it is sent
    Given the test API answers 200 with '{"text":"hello"}'
    When I call GET "/v1/unlisted" with the text "hi"
    Then the call should fail mentioning "no operation of the adapter's list"
    And the test API should have received 0 requests

  @category/happy
  Scenario: The client middleware sees the call and may send it again, each attempt sent and logged
    Given the test API answers 200 with '{"text":"hello"}'
    And a client middleware that sends every call twice
    When I call POST "/v1/echo" with the text "hi"
    Then the middleware should have seen the call "POST /v1/echo" of "test-api", retryable "true"
    And the test API should have received 2 requests
    And the records should be, in order, "http client request sent|http client response received|http client request sent|http client response received"
