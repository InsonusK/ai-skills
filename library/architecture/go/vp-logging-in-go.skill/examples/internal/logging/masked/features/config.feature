@type/tech-check
Feature: Logging settings
  As the operator who deploys the service
  I want the payload rules read from their own environment variables
  So that a busy service logs only the payloads of failed calls without a rebuild

  @category/boundary
  Scenario: Nothing set means every payload, cut at 4096 bytes
    When the logging settings are loaded
    Then LOG_PAYLOAD should be "always" and the limit 4096

  @category/negative
  Scenario Outline: An unusable value is rejected, naming its variable
    Given the environment variable "<name>" is "<value>"
    When the logging settings are loaded
    Then loading should fail mentioning "<name>"

    Examples:
      | name                  | value |
      | LOG_PAYLOAD           | some  |
      | LOG_PAYLOAD_MAX_BYTES | ten   |
      | LOG_PAYLOAD_MAX_BYTES | 10    |
