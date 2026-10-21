@type/tech-check
Feature: Call log
  As an operator of the service
  I want one record per gRPC call with its method, its status code, and the request message
  So that I find any call in the log, and, with the logging variation point, its data

  @category/happy
  Scenario: A call is logged with its method, its code, and the request message
    Given the command answers "hello"
    When I call the test method with "hi"
    Then one "grpc call" record should be logged with the method "/test.v1.EchoService/Echo", the code "OK", and failed "false"
    And its payload should be the message "hi"

  @category/negative
  Scenario: A rejected call is logged as failed
    Given the command is rejected with "item titles repeat"
    When I call the test method with "a"
    Then one "grpc call" record should be logged with the method "/test.v1.EchoService/Echo", the code "InvalidArgument", and failed "true"

  @category/happy
  Scenario: The request id of the client is kept and sent back
    When I call the test method with "a" and the request id "req-42"
    Then the server should send back the request id "req-42"
    And the context of the command should carry the request id "req-42"
