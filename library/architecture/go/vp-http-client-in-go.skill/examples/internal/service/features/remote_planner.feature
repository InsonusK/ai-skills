@type/service
Feature: RemotePlanner service
  As a command that plans with the todo service
  I want the todo API's technical outcomes told in the application's error kinds
  So that an outage is unavailable, rejected titles are invalid, and anything else is a failure of this service

  @category/happy
  Scenario: The items the API planned come back as open items
    Given the todo API plans "buy milk|walk the dog"
    When I plan "buy milk|walk the dog" remotely
    Then the planned items should be the open items "buy milk|walk the dog"

  @category/error
  Scenario: An API that does not answer is unavailable
    Given the todo API is unavailable
    When I plan "a" remotely
    Then planning should fail with the kind "unavailable"

  @category/negative
  Scenario: Titles the API rejects are invalid, with its message
    Given the todo API answers INVALID_ARGUMENT with the message "item titles repeat"
    When I plan "a|a" remotely
    Then planning should fail with the kind "invalid" mentioning "item titles repeat"

  @category/error
  Scenario Outline: Any other answer is a failure with no kind
    Given the todo API <fails>
    When I plan "a" remotely
    Then planning should fail with no kind

    Examples:
      | fails                                                  |
      | answers ALREADY_EXISTS with the message "taken"        |
      | answers INTERNAL with the message "internal error"     |
      | plans ""                                               |
