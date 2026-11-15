@type/service
Feature: The specifications that pick items
  As a handler that asks the store for items
  I want each specification to pick exactly the rows its name says
  So that a handler's question is written once and answered the same over memory and over the database

  Background:
    Given the stored items:
      | list | title        |
      | home | buy milk     |
      | work | buy milk     |
      | home | walk the dog |

  @category/happy
  Scenario: The items of a list are its own, in the order they were added
    When I pick the items of the list "home"
    Then the picked titles should be "buy milk|walk the dog"

  @category/boundary
  Scenario: The items of a list nobody added to are none
    When I pick the items of the list "garden"
    Then nothing should be picked

  @category/happy
  Scenario: A title of a list picks that item only
    When I pick the item "buy milk" of the list "work"
    Then the picked items should be "work: buy milk"

  @category/boundary
  Scenario: A title differing only in case picks nothing
    When I pick the item "Buy milk" of the list "home"
    Then nothing should be picked
