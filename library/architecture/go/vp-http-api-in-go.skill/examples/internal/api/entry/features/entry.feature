@type/tech-check
Feature: Running the entry points together
  As the operator who runs the process
  I want every entry point run together and stopped together
  So that a failed entry point never leaves the process half-alive, and the exit code says how it ended

  @category/happy
  Scenario: A one-shot entry that is done stops the long-running ones
    Given a long-running entry "health"
    And a one-shot entry "startup" that succeeds
    When the entry points run together
    Then every entry point should have stopped
    And running should end without an error
    And the exit code should be 0

  @category/happy
  Scenario: Termination stops every long-running entry without an error
    Given a long-running entry "http"
    And a long-running entry "health"
    When the entry points run together and the process is told to terminate
    Then every entry point should have stopped
    And running should end without an error
    And the exit code should be 0

  @category/error
  Scenario: An entry that fails stops the others, and the process exits 1
    Given a long-running entry "health"
    And an entry "http" that fails with "listen tcp :8080: address already in use"
    When the entry points run together
    Then every entry point should have stopped
    And running should end with the error "listen tcp :8080: address already in use"
    And the exit code should be 1

  @category/negative
  Scenario Outline: The exit code follows the error kind: 1 when a retry may help, 2 otherwise
    Given a one-shot entry "startup" that fails with the kind "<kind>"
    When the entry points run together
    Then the exit code should be <code>

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
