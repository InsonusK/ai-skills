@type/tech-check
Feature: Retry settings
  As the operator who deploys the service
  I want the retry of each client set by the variables of its API's prefix
  So that I change how often and how long a client repeats without a release

  @category/boundary
  Scenario: With none set a call gets 3 attempts, the second after 100ms
    When the retry settings of "TEST_API" are loaded
    Then the retry should allow 3 attempts with a backoff of "100ms"

  @category/happy
  Scenario: Both are read
    Given the environment variable "TEST_API_RETRY_ATTEMPTS" is "5"
    And the environment variable "TEST_API_RETRY_BACKOFF" is "250ms"
    When the retry settings of "TEST_API" are loaded
    Then the retry should allow 5 attempts with a backoff of "250ms"

  @category/negative
  Scenario Outline: An unusable value is rejected, naming its variable
    Given the environment variable "<name>" is "<value>"
    When the retry settings of "TEST_API" are loaded
    Then loading should fail mentioning "<name>"

    Examples:
      | name                    | value |
      | TEST_API_RETRY_ATTEMPTS | 0     |
      | TEST_API_RETRY_ATTEMPTS | many  |
      | TEST_API_RETRY_BACKOFF  | 100   |
      | TEST_API_RETRY_BACKOFF  | 0s    |
