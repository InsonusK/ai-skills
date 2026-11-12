@type/api/http
Feature: HTTP contract
  As a client who reads the published contract
  I want the served routes and the contract's operations to be the same
  So that Swagger UI never describes a route that answers 404

  @category/happy
  Scenario: The HTTP API serves exactly the operations its contract describes
    Then the HTTP API and its OpenAPI contract both have exactly the operations:
      | method | path           |
      | POST   | /v1/items/plan |
