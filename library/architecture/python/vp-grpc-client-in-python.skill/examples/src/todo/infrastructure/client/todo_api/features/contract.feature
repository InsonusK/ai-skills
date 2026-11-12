@type/tech-check
Feature: The todo API's contract
  As a developer of the client of the todo service
  I want every method the adapter calls to be a method of the copied contract
  So that a method the provider removed fails here and not in production

  @category/happy
  Scenario: The adapter calls only methods of docs/integration/client/todo/todo.proto
    Then the adapter should call exactly these methods, each in the contract:
      | method                          | retryable |
      | /todo.v1.ItemsService/PlanItems | yes       |
