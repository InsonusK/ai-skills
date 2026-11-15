@type/api/grpc
Feature: Command call
  As a client of the service
  I want every method to call its command and answer errors the same way
  So that I can tell a rejected call from a failure on any method

  @category/happy
  Scenario: A decoded request reaches the command and the result comes back
    Given the gRPC echo command answers "hello"
    When I call the gRPC test method with "hi"
    Then the gRPC call should succeed with "hello"
    And the gRPC echo command should have received "hi"

  @category/negative
  Scenario: A command rejected by a business rule is INVALID_ARGUMENT with the rule's message
    Given the gRPC echo command is rejected with "item titles repeat"
    When I call the gRPC test method with "a"
    Then the gRPC call should fail with INVALID_ARGUMENT and the message "item titles repeat"

  @category/negative
  Scenario: Every other kind is answered by the error-kinds table
    Given the gRPC echo command fails with the kind "conflict" and "the name is taken"
    When I call the gRPC test method with "a"
    Then the gRPC call should fail with ALREADY_EXISTS and the message "the name is taken"

  @category/negative
  Scenario: An error wrapped on its way up is answered by its kind
    Given the gRPC echo command fails with the kind "not found" and "no list 42", wrapped in "plan failed"
    When I call the gRPC test method with "a"
    Then the gRPC call should fail with NOT_FOUND and the message "no list 42"

  @category/negative
  Scenario: A request that cannot become the command is INVALID_ARGUMENT and never reaches it
    When I call the gRPC test method with ""
    Then the gRPC call should fail with INVALID_ARGUMENT and the message "text is required"
    And the gRPC echo command should not have been called

  @category/error
  Scenario: Any other failure is INTERNAL without the internal message, which goes to the log
    Given the gRPC echo command fails with "connection refused"
    When I call the gRPC test method with "a"
    Then the gRPC call should fail with INTERNAL and the message "internal error"
    And the gRPC log should hold the error "connection refused" of "grpc call failed"
    When the gRPC echo command answers "still here"
    And I call the gRPC test method with "a"
    Then the gRPC call should succeed with "still here"

  @category/error
  Scenario: A command that crashes is INTERNAL and the server keeps serving
    Given the gRPC echo command crashes
    When I call the gRPC test method with "a"
    Then the gRPC call should fail with INTERNAL and the message "internal error"
    When the gRPC echo command answers "still here"
    And I call the gRPC test method with "a"
    Then the gRPC call should succeed with "still here"
