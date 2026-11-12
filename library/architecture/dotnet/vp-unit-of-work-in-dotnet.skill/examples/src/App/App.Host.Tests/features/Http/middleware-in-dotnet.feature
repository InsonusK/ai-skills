@type/tech-check
Feature: Middlewares of the contract routes, in dotnet
  As a developer attaching a variation point that works on requests
  I want a type that is no middleware refused when the entry point is built
  So that a wrong entry of the list fails at startup, not on the first request

  @category/negative
  Scenario: A type that is no middleware is refused when the entry point is built
    When the type "String" is built as an HTTP middleware
    Then building should fail mentioning "String is not an IMiddleware"
