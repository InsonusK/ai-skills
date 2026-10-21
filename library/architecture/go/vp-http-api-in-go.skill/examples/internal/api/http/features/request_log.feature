@type/tech-check
Feature: Request log
  As an operator of the service
  I want one record per HTTP request with what the client sent and how it was answered
  So that I find any request in the log, and, with the logging variation point, its data

  @category/happy
  Scenario: A request is logged with its method, path, status, and what the client sent
    Given the command answers "hello"
    When I POST "/v1/echo?mode=fast" with the body '{"text":"hi"}'
    Then one "http request" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 201    | false  |
    And its payload should hold the query "mode" = "fast" and the body '{"text":"hi"}'
    And the command should have received "hi"

  @category/negative
  Scenario: A rejected request is logged as failed
    Given the command is rejected with "item titles repeat"
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then one "http request" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 400    | true   |

  @category/happy
  Scenario: The request id of the client is kept and sent back
    When I POST "/v1/echo" with the body '{"text":"a"}' and the request id "req-42"
    Then the response should carry the request id "req-42"
    And the context of the command should carry the request id "req-42"

  @category/boundary
  Scenario: A request without an id gets a new one
    When I POST "/v1/echo" with the body '{"text":"a"}'
    Then the response should carry a request id of 16 hex characters
