@type/tech-check
Feature: Base configuration
  As the operator who starts the process
  I want the base settings read from the environment
  So that the startup command and the log level change without a rebuild

  @category/happy
  Scenario: Settings are read from the environment
    Given the environment variable "LOG_LEVEL" is "debug"
    And the environment variable "STARTUP_TITLES" is "buy milk,walk the dog"
    When the configuration is loaded
    Then the log level should be "DEBUG"
    And the startup titles should be "buy milk|walk the dog"

  @category/boundary
  Scenario: Nothing set means info level and no startup titles
    When the configuration is loaded
    Then the log level should be "INFO"
    And there should be no startup titles

  @category/negative
  Scenario: An unknown log level is rejected
    Given the environment variable "LOG_LEVEL" is "loud"
    When the configuration is loaded
    Then loading should fail mentioning "LOG_LEVEL"
