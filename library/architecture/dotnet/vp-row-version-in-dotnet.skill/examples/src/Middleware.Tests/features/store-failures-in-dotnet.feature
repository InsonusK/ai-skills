@type/service
Feature: Store failures told in the error kinds
  As a caller of any command
  I want a failure of the store answered by its kind
  So that no handler reads an engine's code, and none is missed

  @category/happy
  Scenario: A command that succeeds answers its result
    When a command over the store failures middleware returns "done"
    Then the command should answer "done"

  @category/negative
  Scenario: A failure the store tells becomes its kind, the failure its cause
    Given the store tells the failure "duplicate key" as the kind "conflict" with the message "item already exists"
    When a command over the store failures middleware fails with "duplicate key"
    Then the command should fail with the kind "conflict" and the message "item already exists"
    And the cause of the error should be "duplicate key"

  @category/negative
  Scenario: A failure the store does not tell passes as it is
    When a command over the store failures middleware fails with "disk full"
    Then the command should fail with no kind and the message "disk full"
