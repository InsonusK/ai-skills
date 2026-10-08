@service
Feature: Summarize a batch of URLs
  A caller checks many URLs at once and reads the totals.

  @happy
  Scenario: Valid and invalid URLs are counted apart
    Given the URLs:
      | url               |
      | https://a.example |
      | ftp://b.example   |
      | https://          |
    When I summarize the batch
    Then the batch has 1 valid and 2 invalid URLs
    And the count of "UNSUPPORTED_SCHEME" errors is 1
    And the count of "MISSING_HOST" errors is 1

  @boundary
  Scenario: An empty batch has no totals
    Given no URLs
    When I summarize the batch
    Then the batch has 0 valid and 0 invalid URLs

  # todo: the size limit is not agreed with the API owner yet
  @todo @security
  Scenario: A batch over the size limit is refused
    Given a batch of 10001 URLs
    When I summarize the batch
    Then the batch is refused with error "BATCH_TOO_LARGE"
