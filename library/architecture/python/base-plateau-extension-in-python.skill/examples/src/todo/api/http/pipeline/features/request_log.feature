@type/tech-check
Feature: Request log
  As an operator of the service
  I want each HTTP request logged when it arrives and when it is answered, with what came in and what went out
  So that the log reads top-down as the request was served, and, with the logging variation point, shows its data

  @category/happy
  Scenario: The log reads top-down: request received, command called and handled, response sent
    Given the command answers "hello"
    And the command is wrapped with logging
    When I POST "/v1/echo" with the body '{"text":"hi"}'
    Then the records should be, in order, "http request received|command called|command handled|http response sent"

  @category/happy
  Scenario: The received record holds what the client sent, the sent record what it was answered
    Given the command answers "hello"
    When I POST "/v1/echo?mode=fast" with the body '{"text":"hi"}'
    Then the "http request received" record should hold the query "mode" = "fast" and the body '{"text":"hi"}'
    And one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 201    | false  |
    And the "http response sent" record should hold the response body '{"text":"hello"}'
    And the "http response sent" record should hold no request

  @category/negative
  Scenario: A failed request's sent record repeats what the client sent
    Given the command is rejected with "item titles repeat"
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 400    | true   |
    And the "http response sent" record should hold the response body '{"code":3,"message":"item titles repeat","details":[]}'
    And the "http response sent" record should hold the request body '{"text":"a"}'

  @category/happy
  Scenario: The request id of the client is kept and sent back
    Given the command answers "hello"
    When I POST "/v1/echo" with the body '{"text":"a"}' and the request id "req-42"
    Then the response should carry the request id "req-42"
    And the command should have run with the request id "req-42"

  @category/boundary
  Scenario: A request without an id gets a new one
    Given the command answers "hello"
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then the response should carry a request id of 16 hex characters
