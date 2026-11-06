@type/tech-check
Feature: Client settings
  As the operator who deploys the service
  I want every client configured by the variables of its API's prefix
  So that I find, change, or mock any client's connection without reading its code

  @category/boundary
  Scenario: Only the host set means port 80, no TLS, and a timeout of 5s
    Given the environment variable "TEST_API_HOST" is "test.internal"
    When the settings of "TEST_API" are loaded
    Then the base URL should be "http://test.internal:80" and the timeout "5s"

  @category/happy
  Scenario: TLS changes the scheme and the default port; port and timeout are read
    Given the environment variable "TEST_API_HOST" is "test.internal"
    And the environment variable "TEST_API_TLS" is "true"
    And the environment variable "TEST_API_TIMEOUT" is "750ms"
    When the settings of "TEST_API" are loaded
    Then the base URL should be "https://test.internal:443" and the timeout "750ms"

  @category/happy
  Scenario: The port is read
    Given the environment variable "TEST_API_HOST" is "test.internal"
    And the environment variable "TEST_API_HTTP_PORT" is "8080"
    When the settings of "TEST_API" are loaded
    Then the base URL should be "http://test.internal:8080" and the timeout "5s"

  @category/negative
  Scenario Outline: An unusable or missing value is rejected, naming its variable
    Given the environment variable "TEST_API_HOST" is "<host>"
    And the environment variable "<name>" is "<value>"
    When the settings of "TEST_API" are loaded
    Then loading should fail mentioning "<named>"

    Examples:
      | host          | name               | value | named              |
      |               | TEST_API_TLS       | false | TEST_API_HOST      |
      | test.internal | TEST_API_TLS       | maybe | TEST_API_TLS       |
      | test.internal | TEST_API_HTTP_PORT | http  | TEST_API_HTTP_PORT |
      | test.internal | TEST_API_HTTP_PORT | 0     | TEST_API_HTTP_PORT |
      | test.internal | TEST_API_TIMEOUT   | 5     | TEST_API_TIMEOUT   |
      | test.internal | TEST_API_TIMEOUT   | 1h    | TEST_API_TIMEOUT   |
      | test.internal | TEST_API_TIMEOUT   | 0s    | TEST_API_TIMEOUT   |
