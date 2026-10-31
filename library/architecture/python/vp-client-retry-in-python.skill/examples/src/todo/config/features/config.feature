@type/tech-check
Feature: Base configuration
  As the operator who starts the process
  I want the base settings read from the environment
  So that the log level changes without a rebuild

  @category/happy
  Scenario: The log level is read from the environment
    Given the environment variable "LOG_LEVEL" is "debug"
    When the configuration is loaded
    Then the log level should be "DEBUG"

  @category/boundary
  Scenario: Nothing set means info level
    When the configuration is loaded
    Then the log level should be "INFO"

  @category/negative
  Scenario: An unknown log level is rejected
    Given the environment variable "LOG_LEVEL" is "loud"
    When the configuration is loaded
    Then loading should fail mentioning "LOG_LEVEL"
