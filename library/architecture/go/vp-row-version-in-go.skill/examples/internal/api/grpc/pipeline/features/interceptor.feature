@type/tech-check
Feature: Interceptors of the contract's calls
  As a developer attaching a variation point that works on calls
  I want to give the gRPC entry point a list of interceptors around every call of its contract
  So that a step on every call (authentication) is added in the composition root and is logged and answered like any call

  @category/happy
  Scenario: The interceptors wrap a call in the order of the list, inside the call log and around the command
    Given the gRPC interceptors "outer" and "inner", each logging a record with its name
    And the gRPC echo command answers "hello", wrapped with logging
    When I call the gRPC test method with "hi"
    Then the gRPC records should be, in order, "grpc call received|interceptor outer|interceptor inner|command called|command handled|grpc call answered"

  @category/negative
  Scenario: A call an interceptor rejects with an error kind is answered by its kind and logged as failed
    Given a gRPC interceptor that rejects every call as unauthenticated with "no token"
    When I call the gRPC test method with "hi"
    Then the gRPC call should fail with UNAUTHENTICATED and the message "no token"
    And the gRPC echo command should not have been called
    And one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "UNAUTHENTICATED", and failed "true"
