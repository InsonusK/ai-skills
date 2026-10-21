@type/domain
Feature: Plan items
  As the caller of the planner domain service
  I want a list of titles turned into open items
  So that the rules spanning several items are checked in one place

  @category/happy
  Scenario: Plan one item per title, in order
    When I plan the titles "buy milk|walk the dog"
    Then planning should succeed with the titles "buy milk|walk the dog"

  @category/negative
  Scenario Outline: Titles that cannot be planned
    When I plan the titles "<titles>"
    Then planning should fail with "<reason>"

    Examples:
      | titles              | reason          |
      | buy milk\|Buy Milk  | duplicate title |
      | buy milk\| BUY MILK | duplicate title |
      | buy milk\|          | empty title     |

  @category/boundary
  Scenario: Planning nothing is rejected
    When I plan no titles
    Then planning should fail with "no items"
