@type/tech-check
Feature: Access settings
  As the operator of the service
  I want the callers and the batch limit read from the environment, with a clear error for a wrong value
  So that the access variation point is set per deployment without a rebuild

  @category/happy
  Scenario: The callers are read with their roles, the limit has its default
    Given the environment variable "ACCESS_TOKENS" is "t-alice=alice:planner+admin, t-bob=bob"
    When the access settings are loaded
    Then the token "t-alice" should name "alice" with the roles "planner,admin"
    And the token "t-bob" should name "bob" with the roles ""
    And the batch limit should be 100

  @category/negative
  Scenario: A caller without a user stops the start, naming the variable
    Given the environment variable "ACCESS_TOKENS" is "t-alice"
    When the access settings are loaded
    Then loading should fail mentioning "ACCESS_TOKENS"

  @category/negative
  Scenario: A limit below one stops the start, naming the variable
    Given the environment variable "ACCESS_MAX_BATCH" is "0"
    When the access settings are loaded
    Then loading should fail mentioning "ACCESS_MAX_BATCH"
