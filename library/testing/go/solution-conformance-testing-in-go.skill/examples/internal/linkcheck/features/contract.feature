@type/contract
Feature: The shape of a check result
  Callers and the history store depend on the fields of a result; they change only on purpose.

  @category/happy
  Scenario: A result exposes exactly its three fields
    Then a check result has the fields:
      | field      |
      | is_valid   |
      | normalized |
      | error_code |

  @category/regression
  Scenario: A valid result carries no error code
    Given the URL "https://a.example"
    When I check the URL
    Then the error code is empty
