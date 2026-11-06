@type/tech-check
Feature: HTTP settings
  As the operator who deploys the service
  I want the HTTP entry point's settings read from its own environment variables
  So that the port, the published prefix and the docs switch change without a rebuild

  @category/boundary
  Scenario: Nothing set means port 8080, no prefix, no docs
    When the HTTP settings are loaded
    Then the HTTP listen port should be 8080
    And the HTTP base path should be ""
    And the HTTP docs should be disabled

  @category/happy
  Scenario Outline: The prefix always gets one leading slash and no trailing one
    Given the environment variable "HTTP_BASE_PATH" is "<raw>"
    When the HTTP settings are loaded
    Then the HTTP base path should be "<normalized>"

    Examples:
      | raw    | normalized |
      | todo   | /todo      |
      | /todo/ | /todo      |
      | /      |            |

  @category/happy
  Scenario: Port and docs switch are read
    Given the environment variable "HTTP_LISTEN_PORT" is "9090"
    And the environment variable "HTTP_API_DOCS_ENABLED" is "true"
    When the HTTP settings are loaded
    Then the HTTP listen port should be 9090
    And the HTTP docs should be enabled

  @category/negative
  Scenario Outline: An unusable value is rejected, naming its variable
    Given the environment variable "<name>" is "<value>"
    When the HTTP settings are loaded
    Then loading the HTTP settings should fail mentioning "<name>"

    Examples:
      | name                  | value |
      | HTTP_LISTEN_PORT      | http  |
      | HTTP_LISTEN_PORT      | 70000 |
      | HTTP_LISTEN_PORT      | 0     |
      | HTTP_API_DOCS_ENABLED | maybe |
