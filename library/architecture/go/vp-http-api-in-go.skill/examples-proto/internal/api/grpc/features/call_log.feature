@type/tech-check
Feature: Call log
  As an operator of the service
  I want each gRPC call logged when it arrives and when it is answered, with the messages in and out
  So that the log reads top-down as the call was served, and, with the logging variation point, shows its data

  @category/happy
  Scenario: A call is logged when received, with the request, and when answered, with the response
    Given the command answers "hello"
    When I call the test method with "hi"
    Then the records should be, in order, "grpc call received|grpc call answered"
    And the "grpc call received" record should hold the request "hi"
    And one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "OK", and failed "false"
    And the "grpc call answered" record should hold the response "hello" and no request

  @category/negative
  Scenario: A failed call's answered record repeats the request
    Given the command is rejected with "item titles repeat"
    When I call the test method with "a"
    Then one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "InvalidArgument", and failed "true"
    And the "grpc call answered" record should hold the request "a"

  @category/happy
  Scenario: The request id of the client is kept and sent back
    When I call the test method with "a" and the request id "req-42"
    Then the server should send back the request id "req-42"
    And the context of the command should carry the request id "req-42"
