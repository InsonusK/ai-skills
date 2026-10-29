@type/tech-check
Feature: The todo API's contract
  As a developer of the client of the todo service
  I want every operation the adapter calls to be an operation of the copied contract
  So that a mistyped path, or an operation the provider removed, fails here and not in production

  @category/happy
  Scenario: The adapter calls only operations of docs/integration/client/todo/openapi.json
    Then the adapter should call exactly these operations, each in the contract:
      | method | path           | retryable |
      | POST   | /v1/items/plan | yes       |
