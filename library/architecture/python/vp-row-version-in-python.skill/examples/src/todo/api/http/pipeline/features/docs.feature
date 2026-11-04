@type/api/http
Feature: API documentation
  As a developer integrating with the service
  I want the OpenAPI spec and Swagger UI served by the service when the operator enables them
  So that I can read and try the contract, even when the service is published under a path prefix

  @category/happy
  Scenario: With docs enabled under a prefix, every emitted URL carries the prefix
    Given the docs are enabled
    And the service is published under "/todo"
    When I GET "/openapi.json"
    Then the response status should be 200
    And the spec's server URL should be "/todo"
    When I GET "/swagger/"
    Then the response status should be 200
    When I GET "/swagger/swagger-initializer.js"
    Then the response status should be 200
    And the response body should mention "/todo/openapi.json"
    When I GET "/swagger/swagger-ui-bundle.js"
    Then the response status should be 200
    When I GET "/swagger"
    Then the response status should be 301
    And the response should redirect to "/todo/swagger/"

  @category/boundary
  Scenario: Without a prefix the committed spec is served unchanged
    Given the docs are enabled
    When I GET "/openapi.json"
    Then the response should be the committed spec

  @category/security
  Scenario Outline: With docs disabled the documentation routes do not exist
    When I GET "<path>"
    Then the response status should be 404

    Examples:
      | path          |
      | /openapi.json |
      | /swagger/     |
      | /swagger      |
