@type/tech-check
Feature: A mocked client
  As a developer who runs the service without the services it calls
  I want a client whose host is mock answered by its API's emulator inside the process
  So that the service starts and runs its flows with no other service reachable, and the log says so

  @category/happy
  Scenario: With the host mock, the emulator answers and the call is logged as any other
    Given the client of "TEST_API" is mocked by an emulator answering "from the emulator"
    When I call Echo with the text "hi"
    Then the call should succeed with the text "from the emulator"
    And the records should be, in order, "api client mocked|grpc client call sent|grpc client call answered"
    And the "api client mocked" record should be a warning naming the API "test-api"
