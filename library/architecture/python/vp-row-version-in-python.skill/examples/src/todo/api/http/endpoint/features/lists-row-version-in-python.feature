@type/api/http
Feature: Row version of Lists API
  As a client of the service
  I want the lists operations to call their commands with the list from the path
  So that I add and read items over HTTP, with the statuses the contract names


  @category/happy
  Scenario: With row version, pOST /v1/lists/{listId}/items/{itemId}/complete becomes CompleteItem with the version of the body
    Given CompleteItem returns the done item 1 "buy milk" at the version "8"
    When I POST "/v1/lists/home/items/1/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 200
    And the HTTP response body should be '{"item":{"title":"buy milk","done":true,"id":"1","version":"8"}}'
    And CompleteItem should have received the list "home", the item 1, and the version "7"

  @category/negative
  Scenario: With row version, an item changed since the caller read it is 409 ABORTED
    Given CompleteItem fails with the kind concurrent change
    When I POST "/v1/lists/home/items/1/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 409

  @category/negative
  Scenario: With row version, an item id that is no number is 400 INVALID_ARGUMENT, and no command runs
    When I POST "/v1/lists/home/items/abc/complete" to the HTTP API with the body '{"version":"7"}'
    Then the HTTP status should be 400
    And CompleteItem should not have been called
