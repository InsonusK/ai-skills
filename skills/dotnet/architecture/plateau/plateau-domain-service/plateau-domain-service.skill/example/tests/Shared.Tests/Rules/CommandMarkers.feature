Feature: MediatR request markers

  The markers in Shared/MediatR are member-free and carry the request kind
  through to MediatR's own interfaces.

  Scenario: Every marker lives in Shared.MediatR and declares no members
    When the request markers are inspected
    Then each one is in namespace "Shared.MediatR"
    And each one declares no instance members
