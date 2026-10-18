@type/api/http
Feature: Command endpoint
  As a client of the service
  I want every operation to call its command and answer errors the same way
  So that I can tell a rejected request from a failure on any route

  @category/happy
  Scenario: A decoded request reaches the command and the result comes back with the route's status
    Given the command answers "hello"
    When I POST "/v1/echo" with the body '{"text":"hi"}'
    Then the response status should be 201
    And the response body should be '{"text":"hello"}'
    And the command should have received "hi"

  @category/happy
  Scenario: An unknown field in the request is ignored
    Given the command answers "hello"
    When I POST "/v1/echo" with the body '{"text":"hi","priority":1}'
    Then the response status should be 201
    And the command should have received "hi"

  @category/negative
  Scenario: A command rejected by a business rule is 400 INVALID_ARGUMENT with the rule's message
    Given the command is rejected with "item titles repeat"
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then the response status should be 400
    And the error body should have code 3 and a message containing "item titles repeat"

  @category/negative
  Scenario: A body that is not JSON is 400 and never reaches the command
    When I POST "/v1/echo" with the body 'not json'
    Then the response status should be 400
    And the error body should have code 3 and a message containing "not valid JSON"
    And the command should not have been called

  @category/error
  Scenario: Any other failure is 500 INTERNAL without the internal message
    Given the command fails with "connection refused"
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then the response status should be 500
    And the error body should have code 13 and the message "internal error"
