@type/tech-check
Feature: Management entry point of the composition root, in dotnet
  As a developer attaching a variation point that has a readiness check
  I want the check it registers through the framework's registry to reach the management entry point
  So that a dependency down holds readiness back without a line in Program.cs

  @category/happy
  Scenario: The entry point of the composition root answers ready with no check registered
    When the management entry point of the composition root serves
    Then readiness on its port should answer 200 with '{"status":"ok"}'

  @category/error
  Scenario: A check a variation point registered holds readiness back
    Given a variation point registered a readiness check "database" that fails
    When the management entry point of the composition root serves
    Then readiness on its port should answer 503 with '{"status":"unavailable","failed":["database"]}'
