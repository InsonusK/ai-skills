@type/domain
Feature: To-do item
  As the planner of a to-do list
  I want an item to guard its own title and state
  So that no item exists with an unusable title or is completed twice

  Scenario Outline: Create an item
    When I create an item titled "<title>"
    Then the item outcome should be "<outcome>"
    And the item title should be "<stored>"

    @category/happy
    Examples: valid titles
      | title    | outcome | stored   |
      | buy milk | created | buy milk |
      | x        | created | x        |

    @category/negative
    Examples: invalid titles
      | title | outcome     | stored |
      |       | empty title |        |

  @category/happy
  Scenario: Surrounding spaces are not part of the title
    When I create an item titled "  buy milk  "
    Then the item title should be "buy milk"

  @category/negative
  Scenario: A title of spaces only is empty
    When I create an item titled "   "
    Then the item outcome should be "empty title"

  @category/boundary
  Scenario: The longest title is accepted, one more character is not
    When I create an item with a title of 100 characters
    Then the item outcome should be "created"
    When I create an item with a title of 101 characters
    Then the item outcome should be "title too long"

  @category/happy
  Scenario: Complete an open item
    Given an open item titled "buy milk"
    When I complete the item
    Then the item should be done

  @category/negative
  Scenario: An item is completed only once
    Given an open item titled "buy milk"
    And the item is completed
    When I complete the item
    Then completing should fail with "already done"
