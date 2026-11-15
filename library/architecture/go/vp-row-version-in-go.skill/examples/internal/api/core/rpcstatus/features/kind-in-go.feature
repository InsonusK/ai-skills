@type/tech-check
Feature: The kind of a Go error answered by the outermost error of a kind
  As a client of any entry point of a Go service
  I want the kind the service told me, not one it met on the way
  So that a sentinel returned bare and a kind in a cause are answered the same as in the other stacks

  @category/happy
  Scenario: A kind returned bare is answered with its code and its name
    When a command returns the kind "unimplemented" bare, wrapped in "plan items"
    Then the rpc code should be 12
    And the rpc message should be "unimplemented"

  @category/error
  Scenario: The kind of an error's cause is not the kind of the error
    When a command fails with the kind "unavailable" and the message "the planner is unavailable", caused by an error of the kind "invalid"
    Then the rpc code should be 14
    And the rpc message should be "the planner is unavailable"
