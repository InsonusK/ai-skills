@type/tech-check
Feature: Middlewares of the contract routes
  As a developer attaching a variation point that works on requests
  I want to give the HTTP entry point a list of middlewares around its contract routes
  So that a step on every request (authentication) is added in the composition root and is logged and answered like any request

  @category/happy
  Scenario: The middlewares wrap a contract route in the order of the list, inside the request log
    Given the middlewares "outer" and "inner", each logging a record with its name
    And the command is wrapped with logging
    When I POST "/v1/echo" with the body '{"text":"hi"}'
    Then the records should be, in order, "http request received|middleware outer|middleware inner|command called|command handled|http response sent"

  @category/negative
  Scenario: A request a middleware rejects with an error kind is answered as google.rpc.Status and logged as failed
    Given a middleware that rejects every request as unauthenticated with "no token"
    When I POST "/v1/echo" with the body '{"text":"hi"}'
    Then the response status should be 401
    And the error body should have code 16 and the message "unauthenticated: no token"
    And the command should not have been called
    And one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 401    | true   |

  @category/boundary
  Scenario: The docs are outside the middlewares
    Given the docs are enabled
    And a middleware that rejects every request as unauthenticated with "no token"
    When I GET "/openapi.json"
    Then the response status should be 200
