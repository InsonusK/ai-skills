@type/api/http
Feature: Items API
  As a client of the service
  I want each operation of the contract to call its command
  So that I reach the application over HTTP and can tell a rejected request from a failure

  @category/happy
  Scenario: A request becomes the command and the result comes back as JSON
    Given the PlanItems handler returns the items "buy milk|walk the dog"
    When I POST "/v1/items/plan" with the body '{"titles":["buy milk","walk the dog"]}'
    Then the response status should be 200
    And the response body should be '{"items":[{"title":"buy milk","done":false},{"title":"walk the dog","done":false}]}'
    And the PlanItems handler should have received the titles "buy milk|walk the dog"

  @category/happy
  Scenario: An unknown field in the request is ignored
    Given the PlanItems handler returns the items "buy milk"
    When I POST "/v1/items/plan" with the body '{"titles":["buy milk"],"priority":1}'
    Then the response status should be 200

  @category/negative
  Scenario: A command rejected by a business rule is 400 INVALID_ARGUMENT with the rule's message
    Given the PlanItems handler rejects the command with "item titles repeat"
    When I POST "/v1/items/plan" with the body '{"titles":["a","a"]}'
    Then the response status should be 400
    And the error body should have code 3 and a message containing "item titles repeat"

  @category/negative
  Scenario: A body that is not JSON is 400 and never reaches the command
    When I POST "/v1/items/plan" with the body 'not json'
    Then the response status should be 400
    And the error body should have code 3 and a message containing "not valid JSON"
    And the PlanItems handler should not have been called

  @category/error
  Scenario: Any other failure is 500 INTERNAL without the internal message
    Given the PlanItems handler fails with "connection refused"
    When I POST "/v1/items/plan" with the body '{"titles":["a"]}'
    Then the response status should be 500
    And the error body should have code 13 and the message "internal error"

  @category/happy
  Scenario: The HTTP API serves exactly the operations its contract describes
    Then the HTTP API and its OpenAPI contract both have exactly the operations:
      | method | path           |
      | POST   | /v1/items/plan |
