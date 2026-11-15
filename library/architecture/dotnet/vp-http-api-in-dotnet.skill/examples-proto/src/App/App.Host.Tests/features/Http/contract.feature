@type/api/http
Feature: HTTP contract
  As a client who reads the published contract
  I want the served routes and the operations of the spec generated from the proto file to be the same
  So that Swagger UI never describes a route that answers 404

  @category/happy
  Scenario: The HTTP API serves exactly the operations its contract describes
    Then the HTTP API should serve exactly the operations its OpenAPI contract describes
