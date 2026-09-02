Feature: Add item feature

  Scenario: A valid add-item command persists the item and records the user timestamp
    Given a title "buy milk"
    When the add-item command is handled
    Then the result is successful
    And the stored item title is "buy milk"

  Scenario: The title property validator rejects a blank title
    When the title property validator runs on ""
    Then validation fails with error code "Sample.Title.Required"
