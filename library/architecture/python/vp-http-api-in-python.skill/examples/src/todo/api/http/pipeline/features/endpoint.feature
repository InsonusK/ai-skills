@type/api/http
Feature: Command endpoint
  As a client of the service
  I want every operation to call its command and answer errors the same way
  So that I can tell a rejected request from a failure on any route

  @category/happy
  Scenario: A decoded request reaches the command and the result comes back with the route's status
    Given the echo command answers "hello"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi"}'
    Then the HTTP status should be 201
    And the HTTP response body should be '{"text":"hello"}'
    And the echo command should have received "hi"

  @category/boundary
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
  Scenario: An error wrapped on its way up is answered by its kind
    Given the echo command fails with the kind "not found" and "no list 42", wrapped in "plan failed"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 404
    And the HTTP error should have code 5 and the message "no list 42"

  @category/negative
  Scenario: A body that is not JSON is 400 and never reaches the command
    When I POST "/v1/echo" to the HTTP API with the body 'not json'
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "not valid JSON"
    And the echo command should not have been called

  @category/negative
  Scenario: A body without a field the route needs is 400 and never reaches the command
    When I POST "/v1/echo" to the HTTP API with the body '{}'
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "body.text"
    And the echo command should not have been called

  @category/negative
  Scenario: A body not declared as JSON is 400 and never reaches the command
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}' as "text/plain"
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "Content-Type must be application/json"
    And the echo command should not have been called

  @category/error
  Scenario: Any other failure is 500 INTERNAL without the internal message, which goes to the log
    Given the echo command fails with "connection refused"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 500
    And the HTTP error should have code 13 and the message "internal error"
    And the HTTP log should hold the error "connection refused" of "http request failed"

  @category/error
  Scenario: A command that crashes is 500 INTERNAL, logged as failed, and the server keeps serving
    Given the echo command crashes
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 500
    And the HTTP error should have code 13 and the message "internal error"
    And one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 500    | true   |
    When the echo command answers "still here"
    And I POST "/v1/echo" to the HTTP API with the body '{"text":"a"}'
    Then the HTTP status should be 201

  @category/negative
  Scenario Outline: An answer the framework gives itself still has the google.rpc.Status body
    When I GET "<path>" from the HTTP API
    Then the HTTP status should be <status>
    And the HTTP error should have code <code> and a message containing "<path>"
    And one "http response sent" record should be logged with:
      | method | path   | status   | failed |
      | GET    | <path> | <status> | true   |

    Examples:
      | path     | status | code |
      | /nothing | 404    | 5    |
      | /v1/echo | 405    | 12   |
