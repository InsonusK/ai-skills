@type/tech-check
Feature: Answering failures
  As a client of the service
  I want a request whose handler broke answered as google.rpc.Status, not with a cut connection
  So that every answer of the API has the one error format, and the failure is in the log

  @category/error
  Scenario: A route that panics is answered INTERNAL and logged as failed
    Given the command panics
    When I POST "/v1/echo" with the body '{"text":"hi"}'
    Then the response status should be 500
    And the error body should have code 13 and the message "internal error"
    And a "http request panicked" record should be logged
    And one "http response sent" record should be logged with:
      | method | path     | status | failed |
      | POST   | /v1/echo | 500    | true   |
