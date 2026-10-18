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

  @category/negative
  Scenario Outline: The exit code follows the error kind: 1 when a retry may help, 2 otherwise
    When the process starts with a command rejected with the kind "<kind>"
    Then the output should be empty
    And the exit code should be <code>

    Examples:
      | kind                | code |
      | invalid             | 2    |
      | not found           | 2    |
      | conflict            | 2    |
      | concurrent change   | 2    |
      | failed precondition | 2    |
      | unauthenticated     | 2    |
      | permission denied   | 2    |
      | resource exhausted  | 2    |
      | unimplemented       | 2    |
      | unavailable         | 1    |

  @category/happy
  Scenario: The startup input is a comma-separated environment variable
    Given the environment variable "STARTUP_TITLES" is "buy milk,walk the dog"
    When the startup input is read from "STARTUP_TITLES"
    Then the startup input should be "buy milk|walk the dog"

  @category/boundary
  Scenario: An unset startup input is empty
    When the startup input is read from "STARTUP_TITLES"
    Then the startup input should be empty
