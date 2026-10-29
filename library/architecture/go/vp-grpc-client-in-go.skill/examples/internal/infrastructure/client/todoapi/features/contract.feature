@type/tech-check
Feature: The todo API's contract
  As a developer of the client of the todo service
  I want the generated code to be the copied contract's, and every method the adapter calls to be in it
  So that a contract replaced without regenerating, or a method the provider removed, fails here and not in production

  @category/happy
  Scenario: The adapter calls only methods of docs/integration/client/todo/todo.proto
    Then the adapter should call exactly these methods, each in the contract:
      | method                          | retryable |
      | /todo.v1.ItemsService/PlanItems | yes       |

  @category/happy
  Scenario: The generated code describes the copied contract
    Then the code in gen/client/todo should be generated from docs/integration/client/todo/todo.proto
