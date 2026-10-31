@type/api/http
Feature: Items API
  As a client of the service
  I want the items operations of the HTTP form to call their commands
  So that I plan items over HTTP

  @category/happy
  Scenario: POST /v1/items/plan becomes PlanItems and its result comes back as proto-JSON
    Given PlanItems returns the items "buy milk|walk the dog"
    When I POST "/v1/items/plan" to the HTTP API with the body '{"titles":["buy milk","walk the dog"]}'
    Then the HTTP status should be 200
    And the HTTP response body should be '{"items":[{"title":"buy milk"},{"title":"walk the dog"}]}'
    And PlanItems should have received the titles "buy milk|walk the dog"
