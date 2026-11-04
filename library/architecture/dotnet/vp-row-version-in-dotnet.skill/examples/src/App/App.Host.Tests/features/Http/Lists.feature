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

  @category/happy
  Scenario: POST /v1/lists/{listId}/items/{itemId}/complete becomes CompleteItem with the version of the body
    Given CompleteItem returns the done item 1 "buy milk" at the version "8"
    When I POST "/v1/lists/home/items/1/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 200
    And the HTTP response body should be '{"item":{"title":"buy milk","done":true,"id":"1","version":"8"}}'
    And CompleteItem should have received the list "home", the item 1, and the version "7"

  @category/negative
  Scenario: An item changed since the caller read it is 409 ABORTED
    Given CompleteItem fails with the kind concurrent change
    When I POST "/v1/lists/home/items/1/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 409
    And the HTTP error should have code 10 and the message 'item 1 of list "home" was changed since it was read: read it again'

  @category/negative
  Scenario: An item id that is no number is 400 INVALID_ARGUMENT, and no command runs
    Given CompleteItem returns the done item 1 "buy milk" at the version "8"
    When I POST "/v1/lists/home/items/abc/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 400
    And CompleteItem should not have been called

  @category/negative
  Scenario: The deprecated ClearList answers 501 UNIMPLEMENTED
    Given the HTTP API serves the application's routes
    When I POST "/v1/lists/home/clear" to the HTTP API with the body '{}'
    Then the HTTP status should be 501
    And the HTTP error should have code 12 and the message "ClearList is deprecated since 1.1.0"
