@type/tech-check
Feature: Middleware given as one ordered list, in dotnet
  As a developer attaching a variation point
  I want a type that cannot wrap every command refused when it is listed
  So that a wrong middleware fails at startup, not on the first command

  @category/negative
  Scenario: A type that is no open generic pipeline behavior is refused when it is listed
    When the type "String" is listed as a middleware
    Then listing should fail mentioning "String is not an open generic IPipelineBehavior<,>"
