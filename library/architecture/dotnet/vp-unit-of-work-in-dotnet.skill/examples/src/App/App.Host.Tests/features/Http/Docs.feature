@type/api/http
Feature: API documentation
  As a developer integrating with the service
  I want the OpenAPI spec and Swagger UI served from the binary when the operator enables them
  So that I can read and try the contract, even when the service is published under a path prefix

  @category/happy
  Scenario: With docs enabled under a prefix, every emitted URL carries the prefix
    Given the HTTP docs are enabled
    And the service is published under "/todo"
    When I GET "/openapi.json" from the HTTP API
    Then the HTTP status should be 200
    And the spec's server URL should be "/todo"
    When I GET "/swagger/index.js" from the HTTP API
    Then the HTTP status should be 200
    And the HTTP response body should mention "/todo/openapi.json"
    When I GET "/swagger" from the HTTP API
    Then the HTTP status should be 301
    And the HTTP response should redirect to the relative "swagger/index.html"

  @category/boundary
  Scenario: Without a prefix the embedded spec is served unchanged
    Given the HTTP docs are enabled
    When I GET "/openapi.json" from the HTTP API
    Then the HTTP response should be the embedded spec

  @category/security
  Scenario Outline: With docs disabled the documentation routes do not exist
    When I GET "<path>" from the HTTP API
    Then the HTTP status should be 404

    Examples:
      | path                |
      | /openapi.json       |
      | /swagger/index.html |
      | /swagger            |
