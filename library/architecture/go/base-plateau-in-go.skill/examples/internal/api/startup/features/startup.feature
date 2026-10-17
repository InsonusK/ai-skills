@type/api/cli
Feature: Startup command
  As the operator who starts the process
  I want it to run its one command and exit with a code that says how it went
  So that a script or a scheduler can tell success, a rejected input and a failure apart

  @category/happy
  Scenario: The result is written as one JSON line and the process exits 0
    When the process starts with PlanItems for the titles "buy milk|walk the dog"
    Then the output should be the JSON line '{"items":[{"title":"buy milk","done":false},{"title":"walk the dog","done":false}]}'
    And the exit code should be 0

  @category/negative
  Scenario: A command rejected by a business rule exits 2 and writes nothing
    When the process starts with PlanItems for the titles "buy milk|buy milk"
    Then the output should be empty
    And the exit code should be 2

  @category/error
  Scenario: A command that fails for another reason exits 1 and writes nothing
    When the process starts with a command whose handler fails with "store unavailable"
    Then the output should be empty
    And the exit code should be 1
