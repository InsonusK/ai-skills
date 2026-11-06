@type/service
Feature: PlanRemoteItems command
  As an entry point of the application
  I want to send PlanRemoteItems and get the items the todo service planned
  So that I plan with the todo service without knowing how it is called

  @category/happy
  Scenario: The planned items come back as views, in order
    Given the remote planner plans "buy milk|walk the dog"
    When I send PlanRemoteItems with the titles "buy milk|walk the dog"
    Then the remote result should list the open items "buy milk|walk the dog"
    And the remote planner should have been given "buy milk|walk the dog"

  @category/negative
  Scenario Outline: A kind of the remote planner comes back unchanged
    Given the remote planner fails with the kind "<kind>"
    When I send PlanRemoteItems with the titles "a"
    Then the command should fail with the kind "<kind>"

    Examples:
      | kind        |
      | invalid     |
      | unavailable |
