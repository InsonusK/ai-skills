@type/api/grpc
Feature: Health service
  As the orchestrator that probes the service
  I want liveness and readiness answered by the standard gRPC health service
  So that a Kubernetes grpc probe restarts only a dead process and routes traffic only to a ready one

  @category/happy
  Scenario Outline: Liveness answers without checking any dependency
    Given a readiness check "database" that fails
    When I ask the health service about "<service>"
    Then the health status should be SERVING

    Examples:
      | service  |
      | liveness |
      |          |

  @category/happy
  Scenario: Readiness is SERVING when every check passes
    Given a readiness check "database" that passes
    When I ask the health service about "readiness"
    Then the health status should be SERVING

  @category/negative
  Scenario: Readiness is NOT_SERVING when a check fails
    Given a readiness check "database" that passes
    And a readiness check "billing" that fails
    When I ask the health service about "readiness"
    Then the health status should be NOT_SERVING

  @category/negative
  Scenario: An unknown health service is NOT_FOUND
    When I ask the health service about "orders"
    Then the health call should fail with NotFound
