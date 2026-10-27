@type/tech-check
Feature: Client call log
  As an operator of the service
  I want each call to another API logged when it is sent and when it is answered, with the data in and out
  So that a failed call to another service can be found and read in the log

  @category/happy
  Scenario: A call is logged when sent, with the request, and when answered, with the response
    Given the test API answers "hello"
    When I call Echo with the text "hi"
    Then the records should be, in order, "grpc client call sent|grpc client call answered"
    And the "grpc client call sent" record should hold the request text "hi"
    And one "grpc client call answered" record should be logged with the API "test-api", the method "/test.v1.EchoService/Echo", the code "OK", and failed "false"
    And the "grpc client call answered" record should hold the response text "hello" and no request

  @category/negative
  Scenario: A failed call's answered record holds the status and repeats the request
    Given the test API fails with INVALID_ARGUMENT and the message "text is required"
    When I call Echo with the text "hi"
    Then one "grpc client call answered" record should be logged with the API "test-api", the method "/test.v1.EchoService/Echo", the code "InvalidArgument", and failed "true"
    And the "grpc client call answered" record should hold the request text "hi"
    And the "grpc client call answered" record should hold the status message "text is required"

  @category/error
  Scenario: A call the API never answered is logged as failed with UNAVAILABLE
    Given the test API is not listening
    When I call Echo with the text "hi"
    Then one "grpc client call answered" record should be logged with the API "test-api", the method "/test.v1.EchoService/Echo", the code "Unavailable", and failed "true"
