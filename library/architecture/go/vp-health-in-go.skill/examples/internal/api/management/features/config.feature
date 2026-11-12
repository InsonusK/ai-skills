@type/tech-check
Feature: Management settings
  As the operator who deploys the service
  I want the management entry point's port read from its own environment variable
  So that the probes' port changes without a rebuild

  @category/boundary
  Scenario: Nothing set means port 8081
    When the management settings are loaded
    Then the management listen port should be 8081

  @category/happy
  Scenario: The port is read
    Given the environment variable "MANAGEMENT_LISTEN_PORT" is "9091"
    When the management settings are loaded
    Then the management listen port should be 9091

  @category/negative
  Scenario Outline: An unusable port is rejected, naming its variable
    Given the environment variable "MANAGEMENT_LISTEN_PORT" is "<value>"
    When the management settings are loaded
    Then loading the management settings should fail mentioning "MANAGEMENT_LISTEN_PORT"

    Examples:
      | value |
      | mgmt  |
      | 70000 |
      | 0     |
