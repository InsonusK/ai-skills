@type/service
Feature: PlanItems command
  As an entry point of the application
  I want to send PlanItems and get the planned items back
  So that I never touch the domain to plan a to-do list

  @category/happy
  Scenario: Planned items come back as open views, in order
    When I send PlanItems with the titles "buy milk|walk the dog"
    Then the result should list the open items "buy milk|walk the dog"

  @category/negative
  Scenario Outline: A command that breaks a business rule is rejected as invalid
    When I send PlanItems with the titles "<titles>"
    Then the command should be rejected as invalid

    Examples:
      | titles               |
      | buy milk\|BUY MILK   |
      | buy milk\|           |
