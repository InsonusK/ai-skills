@type/tech-check
Feature: Health endpoints
  As the orchestrator that runs the service
  I want separate liveness and readiness endpoints
  So that I restart a hung process but only hold traffic back while a dependency is down

  @category/happy
  Scenario: Liveness answers without checking any dependency
    Given a readiness check "database" that fails
    When I GET "/health/live" from the management port
    Then the management status should be 200
    And the management response body should be '{"status":"ok"}'

  @category/happy
  Scenario: Readiness answers 200 when every check passes
    Given a readiness check "database" that passes
    When I GET "/health/ready" from the management port
    Then the management status should be 200

  @category/error
  Scenario: Readiness answers 503, names the checks that failed, and logs why
    Given a readiness check "database" that passes
    And a readiness check "user-api" that fails
    When I GET "/health/ready" from the management port
    Then the management status should be 503
    And the management response body should be '{"status":"unavailable","failed":["user-api"]}'
    And the failed readiness check "user-api" should be logged

  @category/error
  Scenario: A check that throws counts as failed
    Given a readiness check "broker" that throws
    When I GET "/health/ready" from the management port
    Then the management status should be 503
    And the failed readiness check "broker" should be logged

  @category/boundary
  Scenario: A degraded dependency still takes traffic
    Given a readiness check "cache" that is degraded
    When I GET "/health/ready" from the management port
    Then the management status should be 200

  @category/happy
  Scenario: The health endpoint reports the version of the running binary
    Given the service version is "1.2.3"
    When I GET "/health" from the management port
    Then the management status should be 200
    And the management response body should be '{"status":"ok","version":"1.2.3"}'
