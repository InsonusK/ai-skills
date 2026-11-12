@type/mapping
Feature: Map a check result to a stored record
  The history store keeps plain records; a result must survive the way there and back.

  @category/happy
  Scenario Outline: A result survives the round trip
    Given the URL "<url>"
    And I check the URL
    When I map the result to a record and back
    Then the result is unchanged

    Examples: a valid and an invalid result
      | url               |
      | https://a.example |
      | ftp://a.example   |

  @category/negative
  Scenario: A record without its validity flag is refused
    Given the record:
      | field      | value             |
      | normalized | https://a.example |
    When I map the record to a result
    Then the mapping fails with error "MISSING_FIELD"
