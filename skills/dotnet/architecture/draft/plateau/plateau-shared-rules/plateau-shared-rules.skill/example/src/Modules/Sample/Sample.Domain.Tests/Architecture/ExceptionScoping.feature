Feature: Exception scoping

  DomainException is a ValueObject/Entity concern. It must only be constructed inside
  Sample.Domain.ValueObjects or Sample.Domain.Entities, never from application handlers,
  validators, pipeline behaviors, or infrastructure adapters.

  Scenario: DomainException_IsThrownOnlyFromValueObjectsOrEntities
    Given the compiled Sample.Domain assembly is loaded
    When every type outside ValueObjects and Entities is scanned for DomainException construction
    Then no violating type or method is found
