Feature: Greeting rule

  Scenario Outline: Valid name produces a greeting
    Given the name "<name>"
    When the greeting is produced
    Then the result is "Hello, <name>!"

    Examples:
      | name    |
      | World   |
      | Plateau |

  Scenario: Empty name throws
    Given the name ""
    When the greeting is produced
    Then an argument exception is thrown
