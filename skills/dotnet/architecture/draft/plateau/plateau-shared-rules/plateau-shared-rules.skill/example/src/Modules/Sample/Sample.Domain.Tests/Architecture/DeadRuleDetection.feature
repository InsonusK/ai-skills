Feature: Dead rule detection

  Every centralized rule must be called from production code outside Sample.Domain.Rules.
  A rule that no adapter (ValueObject, Entity, PropertyValidator, DtoValidator) redirects to
  is dead code and must be removed or wired.

  Scenario: EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules
    Given there are public static Check methods on Sample.Domain.Rules
    When production code outside Sample.Domain.Rules is scanned for calls to them
    Then every Check method is called at least once
