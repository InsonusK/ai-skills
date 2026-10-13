@type/domain
Feature: Link count formatting
  @category/happy
  Scenario Outline: Format a valid link count
    When I format <count> links
    Then the count heading is "<heading>"
    Examples:
      | count | heading |
      | 0     | 0 links |
      | 1     | 1 link  |
      | 2     | 2 links |
  @category/negative
  Scenario: Reject a negative count
    When I format a negative link count
    Then the count is rejected
