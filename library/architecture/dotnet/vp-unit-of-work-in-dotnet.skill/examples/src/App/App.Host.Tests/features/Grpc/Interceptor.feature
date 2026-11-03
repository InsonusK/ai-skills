@type/tech-check
Feature: Interceptors of the contract's calls
  As a developer attaching a variation point that works on calls
  I want to give the gRPC entry point a list of interceptors around every call of its contract
  So that a step on every call (authentication) is added in the composition root and is logged and answered like any call

  @category/happy
  Scenario: The interceptors wrap a call in the order of the list, inside the call log
    Given the gRPC interceptors "outer" and "inner", each logging a record with its name
    And the gRPC echo command answers "hello"
    When I call the gRPC test method with "hi"
    Then the gRPC records should be, in order, "grpc call received|interceptor outer|interceptor inner|grpc call answered"

  @category/negative
  Scenario: A call an interceptor rejects with an error kind is answered by its kind and logged as failed
    Given a gRPC interceptor that rejects every call as unauthenticated with "no token"
    When I call the gRPC test method with "hi"
    Then the gRPC call should fail with Unauthenticated and the message "no token"
    And the gRPC echo command should not have been called
    And one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "Unauthenticated", and failed "True"

  @category/negative
  Scenario: A type that is no interceptor is refused when the entry point is built
    When the type "String" is built as a gRPC interceptor
    Then building the gRPC interceptors should fail mentioning "String is not an Interceptor"
