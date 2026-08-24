# This feature is documentary only — no step definitions bind to it.
# The actual proof is GuardedProperties_AreOnlyWrittenByMembersThatCallTheirRequiredRuleChecks,
# in GuardedPropertyRuleCoverageTests.cs, in this same folder.

Feature: Entity properties guarded by a rule are only written by members that call that rule

  Scenario: GuardedProperties_AreOnlyWrittenByMembersThatCallTheirRequiredRuleChecks
    A rule that checks a property is correct only if every public or internal
    write to that property actually calls the rule. A single registry entry per
    guarded property covers every current and future member that writes it.
