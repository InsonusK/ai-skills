@type/service
Feature: ImportItems
  As a caller with several titles
  I want each added by the command that adds an item
  So that an import follows the same rules as a single add

  @category/happy
  Scenario: Each title runs AddItem, in order
    When I send ImportItems to the list "home" with the titles "buy milk|walk the dog"
    Then AddItem should have run for "buy milk|walk the dog"
    And the import should answer the items "buy milk|walk the dog"

  @category/negative
  Scenario: A rejected title stops the import with its error
    Given the title "walk the dog" is rejected
    When I send ImportItems to the list "home" with the titles "buy milk|walk the dog|call mum"
    Then AddItem should have run for "buy milk"
    And the import should fail with the kind of the rejection
