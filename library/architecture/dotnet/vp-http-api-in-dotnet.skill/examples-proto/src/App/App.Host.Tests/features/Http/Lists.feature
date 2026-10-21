@type/api/http
Feature: Lists API
  As a client of the service
  I want the lists operations to call their commands with the list from the path
  So that I add and read items over HTTP, with the statuses the contract names

  @category/happy
  Scenario: POST /v1/lists/{listId}/items becomes AddItem and answers 201
    Given AddItem returns the item "buy milk"
    When I POST "/v1/lists/home/items" to the HTTP API with the body '{"title":"buy milk"}'
    Then the HTTP status should be 201
    And the HTTP response body should be '{"item":{"title":"buy milk"}}'
    And AddItem should have received the list "home" and the title "buy milk"

  @category/negative
  Scenario: A title already in the list is 409 ALREADY_EXISTS
    Given AddItem fails with the kind "conflict" and 'list "home" already has an item "buy milk"'
    When I POST "/v1/lists/home/items" to the HTTP API with the body '{"title":"buy milk"}'
    Then the HTTP status should be 409
    And the HTTP error should have code 6 and the message 'list "home" already has an item "buy milk"'

  @category/happy
  Scenario: GET /v1/lists/{listId}/items becomes ListItems; a GET has no body
    Given ListItems returns the items "buy milk|walk the dog"
    When I GET "/v1/lists/home/items" from the HTTP API
    Then the HTTP status should be 200
    And the HTTP response body should be '{"items":[{"title":"buy milk"},{"title":"walk the dog"}]}'
    And ListItems should have received the list "home"

  @category/negative
  Scenario: The deprecated ClearList answers 501 UNIMPLEMENTED
    Given the HTTP API serves the application's routes
    When I POST "/v1/lists/home/clear" to the HTTP API with the body '{}'
    Then the HTTP status should be 501
    And the HTTP error should have code 12 and the message "ClearList is deprecated since 1.1.0"
