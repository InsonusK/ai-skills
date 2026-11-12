@type/tech-check
Feature: Client call log
  As an operator of the service
  I want each call to another API logged when it is sent and when it is answered, with the data in and out
  So that a failed call to another service can be found and read in the log

  @category/happy
  Scenario: A call is logged when sent, with the request, and when answered, with the response
    Given the test API answers 200 with '{"text":"hello"}'
    When I call POST "/v1/echo" with the text "hi"
    Then the records should be, in order, "http client request sent|http client response received"
    And the "http client request sent" record should hold the request text "hi"
    And one "http client response received" record should be logged with the API "test-api", POST "/v1/echo", the status 200, and failed "false"
    And the "http client response received" record should hold the response '{"text":"hello"}' and no request

  @category/negative
  Scenario: A failed call's answered record repeats the request
    Given the test API answers 400 with '{"code":3,"message":"text is required","details":[]}'
    When I call POST "/v1/echo" with the text "hi"
    Then one "http client response received" record should be logged with the API "test-api", POST "/v1/echo", the status 400, and failed "true"
    And the "http client response received" record should hold the request text "hi"

  @category/error
  Scenario: A call the API never answered is logged as failed with no status
    Given the test API is not listening
    When I call POST "/v1/echo" with the text "hi"
    Then one "http client response received" record should be logged with the API "test-api", POST "/v1/echo", the status 0, and failed "true"
