# This feature is documentary only — no step definitions bind to it.
# The actual proof is DomainException_IsThrownOnlyFromValueObjectsOrEntities,
# in SampleArchitectureTests.cs, in this same folder.

Feature: DomainException is thrown only from ValueObjects or Entities

  Scenario: DomainException_IsThrownOnlyFromValueObjectsOrEntities
    DomainException represents an invariant violation. It must only be constructed
    from ValueObjects or Entities so that the rest of the application cannot
    bypass the domain layer's own guard.
