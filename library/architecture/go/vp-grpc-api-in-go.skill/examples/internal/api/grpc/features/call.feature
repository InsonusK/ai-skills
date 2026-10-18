@type/api/grpc
Feature: Command call
  As a client of the service
  I want every method to call its command and answer errors the same way
  So that I can tell a rejected call from a failure on any method

  @category/happy
  Scenario: A decoded request reaches the command and the result comes back
    Given the command answers "hello"
    When I call the test method with "hi"
    Then the call should succeed with "hello"
    And the command should have received "hi"

  @category/negative
  Scenario: A command rejected by a business rule is INVALID_ARGUMENT with the rule's message
    Given the command is rejected with "item titles repeat"
    When I call the test method with "a"
    Then the call should fail with InvalidArgument and a message containing "item titles repeat"

  @category/negative
  Scenario: A request that cannot become the command is INVALID_ARGUMENT and never reaches it
    When I call the test method with ""
    Then the call should fail with InvalidArgument and a message containing "text is required"
    And the command should not have been called

  @category/error
  Scenario: Any other failure is INTERNAL without the internal message
    Given the command fails with "connection refused"
    When I call the test method with "a"
    Then the call should fail with Internal and the message "internal error"

  @category/error
  Scenario: A command that panics is INTERNAL and the server keeps serving
    Given the command panics
    When I call the test method with "a"
    Then the call should fail with Internal and the message "internal error"
    When the command answers "still here"
    And I call the test method with "a"
    Then the call should succeed with "still here"
