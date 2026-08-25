# This feature is documentary only — no step definitions bind to it.
# The actual proof is EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules,
# in SampleArchitectureTests.cs, in this same folder.

Feature: Every centralized rule's Check() is actually called by production code

  Scenario: EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules
    A Check() extension declared in Domain.Rules that no production code outside
    Domain.Rules ever calls is dead weight — its own scenario proves the predicate
    is correct, but proves nothing about whether anything actually uses it.
