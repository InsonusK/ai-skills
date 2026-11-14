@type/api/cli
Feature: items commands
  As a person or a script that plans to-do items from the command line
  I want `todo items plan --title ...` to call PlanItems
  So that I plan items without the HTTP API

  @category/happy
  Scenario: Every --title becomes a title of PlanItems, and the result is one JSON line
    Given PlanItems returns the items "buy milk|walk the dog"
    When I run "items plan --title buy-milk --title walk-the-dog"
    Then stdout should be the line '{"items":[{"title":"buy milk","done":false},{"title":"walk the dog","done":false}]}'
    And PlanItems should have received the titles "buy-milk|walk-the-dog"

  @category/happy
  Scenario: --output text prints one checkbox line per item
    Given PlanItems returns the items "buy milk|walk the dog"
    When I run "items plan --title a --output text"
    Then stdout should be the lines "[ ] buy milk|[ ] walk the dog"
