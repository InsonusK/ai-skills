@type/tech-check
Feature: The todo API's contract, in Python
  As a developer of the client of the todo service
  I want the generated code to be the copied contract's
  So that a contract replaced without regenerating fails here and not in production

  @category/happy
  Scenario: The generated code describes the copied contract
    Then the code in src/todo/gen/client/todo should be generated from docs/integration/client/todo/todo.proto
