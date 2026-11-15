@type/api/http
Feature: Published HTTP operations in python
  The application fixes its operations separately from the shared contract check.

  @category/happy
  Scenario: The example publishes its intended HTTP operations
    Then the HTTP API and its OpenAPI contract both have exactly the operations:
      | method | path |
      | POST | /v1/items/plan |
      | POST | /v1/lists/{listId}/items |
      | GET | /v1/lists/{listId}/items |
      | POST | /v1/lists/{listId}/clear |
