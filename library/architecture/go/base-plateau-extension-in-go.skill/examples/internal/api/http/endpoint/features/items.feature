@type/api/http
Feature: Items API
  As a client of the service
  I want the items operations to call their commands
  So that I plan items over HTTP

  @category/happy
  Scenario: POST /v1/items/plan becomes PlanItems and its result comes back as JSON
    Given PlanItems returns the items "buy milk|walk the dog"
    When I POST "/v1/items/plan" with the body '{"titles":["buy milk","walk the dog"]}'
    Then the response status should be 200
    And the response body should be '{"items":[{"title":"buy milk","done":false},{"title":"walk the dog","done":false}]}'
    And PlanItems should have received the titles "buy milk|walk the dog"
