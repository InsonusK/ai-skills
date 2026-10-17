@type/api/http
Feature: Health endpoints
  As the orchestrator that runs the service
  I want separate liveness and readiness endpoints
  So that I restart a hung process but only hold traffic back while a dependency is down

  @category/happy
  Scenario: Liveness answers without checking any dependency
    Given a readiness check "database" that fails
    When I GET "/health/live"
    Then the response status should be 200

  @category/happy
  Scenario: Readiness answers 200 when every check passes
    Given a readiness check "database" that passes
    When I GET "/health/ready"
    Then the response status should be 200

  @category/error
  Scenario: Readiness answers 503 and names the checks that failed
    Given a readiness check "database" that passes
    And a readiness check "user-api" that fails
    When I GET "/health/ready"
    Then the response status should be 503
    And the response body should be '{"failed":["user-api"],"status":"unavailable"}'

  @category/happy
  Scenario: The health endpoint reports the version of the running binary
    Given the service version is "1.2.3"
    When I GET "/health"
    Then the response status should be 200
    And the response body should be '{"status":"ok","version":"1.2.3"}'
