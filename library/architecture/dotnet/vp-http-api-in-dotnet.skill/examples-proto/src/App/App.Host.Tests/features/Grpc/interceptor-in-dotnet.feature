@type/tech-check
Feature: Interceptors of the contract's calls, in dotnet
  As a developer attaching a variation point that works on calls
  I want a type that is no interceptor refused when the entry point is built
  So that a wrong entry of the list fails at startup, not on the first call

  @category/negative
  Scenario: A type that is no interceptor is refused when the entry point is built
    When the type "String" is built as a gRPC interceptor
    Then building the gRPC interceptors should fail mentioning "String is not an Interceptor"
