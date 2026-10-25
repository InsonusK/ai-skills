@type/tech-check
Feature: Middlewares of the contract routes
  As a developer attaching a variation point that works on requests
  I want to give the HTTP entry point a list of middlewares around its contract routes
  So that a step on every request (authentication) is added in the composition root and is logged and answered like any request

  @category/happy
  Scenario: The middlewares wrap a contract route in the order of the list, inside the request log
    Given the HTTP middlewares "outer" and "inner", each logging a record with its name
    And the echo command is wrapped with logging
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi"}'
    Then the HTTP records should be, in order, "http request received|middleware outer|middleware inner|command called|command handled|http response sent"

  @category/negative
  Scenario: A request a middleware rejects with an error kind is answered as google.rpc.Status and logged as failed
    Given an HTTP middleware that rejects every request as unauthenticated with "no token"
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi"}'
    Then the HTTP status should be 401
    And the HTTP error should have code 16 and the message "no token"
    And the echo command should not have been called
    And one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 401    | True   |

  @category/boundary
  Scenario: The docs are outside the middlewares
    Given the HTTP docs are enabled
    And an HTTP middleware that rejects every request as unauthenticated with "no token"
    When I GET "/openapi.json" from the HTTP API
    Then the HTTP status should be 200

  @category/negative
  Scenario: A type that is no middleware is refused when the entry point is built
    When the type "String" is built as an HTTP middleware
    Then building should fail mentioning "String is not an IMiddleware"
