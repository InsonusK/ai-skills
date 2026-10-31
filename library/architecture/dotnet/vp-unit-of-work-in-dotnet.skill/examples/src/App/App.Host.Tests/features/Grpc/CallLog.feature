@type/tech-check
Feature: Call log
  As an operator of the service
  I want each gRPC call logged when it arrives and when it is answered, with the messages in and out
  So that the log reads top-down as the call was served, and, with the logging variation point, shows its data

  @category/happy
  Scenario: A call is logged when received, with the request, and when answered, with the response
    Given the gRPC echo command answers "hello"
    When I call the gRPC test method with "hi"
    Then the gRPC records should be, in order, "grpc call received|grpc call answered"
    And the "grpc call received" record should hold the request "hi"
    And one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "OK", and failed "False"
    And the "grpc call answered" record should hold the response "hello" and no request

  @category/negative
  Scenario: A failed call's answered record repeats the request
    Given the gRPC echo command is rejected with "item titles repeat"
    When I call the gRPC test method with "a"
    Then one "grpc call answered" record should be logged with the method "/test.v1.EchoService/Echo", the code "InvalidArgument", and failed "True"
    And the "grpc call answered" record should hold the request "a"

  @category/happy
  Scenario: The request id of the client is kept and sent back
    Given the gRPC echo command answers "hello"
    When I call the gRPC test method with "a" and the request id "req-42"
    Then the gRPC server should send back the request id "req-42"
    And the gRPC echo command should have run with the request id "req-42"
