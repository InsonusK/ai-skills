@type/api/http
Feature: Command endpoint
  As a client of the service
  I want every operation to call its command and answer errors the same way
  So that I can tell a rejected request from a failure on any route

  @category/happy
  Scenario: A bound request reaches the command and the result comes back with the route's status
    Given the echo command answers "hello"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi"}'
    Then the HTTP status should be 201
    And the HTTP response body should be '{"text":"hello"}'
    And the echo command should have received "hi"

  @category/happy
  Scenario: An unknown field in the request is ignored
    Given the echo command answers "hello"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi","priority":1}'
    Then the HTTP status should be 201
    And the echo command should have received "hi"

  @category/negative
  Scenario: A command rejected by a business rule is 400 INVALID_ARGUMENT with the rule's message
    Given the echo command is rejected with "item titles repeat"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and the message "item titles repeat"

  @category/negative
  Scenario: Every other kind is answered by the error-kinds table
    Given the echo command fails with the kind "conflict" and "the name is taken"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 409
    And the HTTP error should have code 6 and the message "the name is taken"

  @category/negative
  Scenario: A body that is not JSON is 400 and never reaches the command
    When I POST "/v1/echo" to the HTTP API with the body 'not json'
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "not valid JSON"
    And the echo command should not have been called

  @category/negative
  Scenario: A body not declared as JSON is 400 and never reaches the command
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}' as "text/plain"
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "Content-Type must be application/json"
    And the echo command should not have been called

  @category/error
  Scenario: Any other failure is 500 INTERNAL without the internal message
    Given the echo command fails with "connection refused"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 500
    And the HTTP error should have code 13 and the message "internal error"
    And the HTTP log should hold the error "connection refused" of "http request failed"

  @category/negative
  Scenario Outline: An error ASP.NET answers itself still has the google.rpc.Status body
    When I GET "<path>" from the HTTP API
    Then the HTTP status should be <status>
    And the HTTP error should have code <code> and a message containing "<path>"
    And one "http response sent" record should be logged with:
      | method | path   | status   | failed |
      | GET    | <path> | <status> | True   |

    Examples:
      | path     | status | code |
      | /nothing | 404    | 5    |
      | /v1/echo | 405    | 12   |
