Feature: ItemTitle rule

  The centralized title condition, proven once against its own Check().

  Scenario: A blank title fails the required rule
    When the title "" is checked
    Then the check fails with error code "Sample.ItemTitle.Required"

  Scenario: An over-long title fails the length rule
    When a 101-character title is checked
    Then the check fails with error code "Sample.ItemTitle.TooLong"

  Scenario: A normal title passes
    When the title "buy milk" is checked
    Then the check passes
