@type/api/http
Feature: Access to the HTTP API
  As the owner of the service
  I want a request identified by its bearer token, and a command run only for a caller allowed to
  So that the API is closed to unknown callers and every rejection is logged and answered as google.rpc.Status

  Background:
    Given the callers "t-alice=alice:planner,t-bob=bob" and the batch limit 3

  @category/happy
  Scenario: A planner plans items
    When "t-alice" POSTs "/v1/items/plan" with the body '{"titles":["buy milk"]}'
    Then the response status should be 200
    And the records should be, in order, "http request received|command called|command handled|http response sent"

  @category/negative
  Scenario: A request without a token reaches the command, which needs a caller
    When nobody POSTs "/v1/items/plan" with the body '{"titles":["buy milk"]}'
    Then the response status should be 401
    And the error body should have code 16 and the message "unauthenticated: PlanItems needs a caller"
    And the records should be, in order, "http request received|command called|command failed|http response sent"

  @category/negative
  Scenario: A token of no known caller is rejected before the command
    When "t-mallory" POSTs "/v1/items/plan" with the body '{"titles":["buy milk"]}'
    Then the response status should be 401
    And the error body should have code 16 and the message "unauthenticated: the bearer token names no known caller"
    And the records should be, in order, "http request received|http response sent"

  @category/negative
  Scenario: A caller without the role is denied
    When "t-bob" POSTs "/v1/items/plan" with the body '{"titles":["buy milk"]}'
    Then the response status should be 403
    And the error body should have code 7 and the message "permission denied: PlanItems needs the role planner"

  @category/negative
  Scenario: A batch above the limit is rejected after the access check
    When "t-alice" POSTs "/v1/items/plan" with the body '{"titles":["a","b","c","d"]}'
    Then the response status should be 429
    And the error body should have code 8 and the message "resource exhausted: PlanItems takes at most 3 items at once, got 4"

  @category/negative
  Scenario: The access check runs before the batch limit
    When "t-bob" POSTs "/v1/items/plan" with the body '{"titles":["a","b","c","d"]}'
    Then the response status should be 403
