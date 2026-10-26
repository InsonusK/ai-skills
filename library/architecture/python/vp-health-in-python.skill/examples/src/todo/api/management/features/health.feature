@type/tech-check
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
    And the log should hold the warning "readiness check failed" for the check "user-api"

  @category/happy
  Scenario: The health endpoint reports the version of the running application
    Given the service version is "1.2.3"
    When I GET "/health"
    Then the response status should be 200
    And the response body should be '{"status":"ok","version":"1.2.3"}'

  @category/negative
  Scenario Outline: Anything but a GET of a health endpoint is refused
    When I send "<method>" "<path>"
    Then the response status should be <status>

    Examples:
      | method | path          | status |
      | GET    | /nothing      | 404    |
      | POST   | /health/ready | 405    |
