Feature: Guarded property rule coverage

  Every Entity write of a rule-guarded property must be reachable only through a code path
  that calls the corresponding centralized rule Check. This is enforced by a recursive
  call-graph walk over compiled IL, with a registry of guarded properties kept in the
  test project.

  Scenario: GuardedProperties_AreOnlyWrittenByMembersThatCallTheirRequiredRuleChecks
    Given the compiled Sample.Domain assembly is loaded
    When every Entity method that writes a guarded property is analyzed
    Then each write path calls the required rule Check registered for that property
