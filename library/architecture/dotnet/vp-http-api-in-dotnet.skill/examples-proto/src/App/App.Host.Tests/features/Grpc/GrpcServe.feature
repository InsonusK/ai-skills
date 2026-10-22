@type/tech-check
Feature: gRPC server lifecycle
  As the orchestrator that stops the service
  I want the gRPC server to stop cleanly when the process is told to terminate
  So that a rolling update does not cut calls off or report a failure

  @category/happy
  Scenario: The server stops without an error when it is told to stop
    Given the gRPC server is serving on a free port
    When the gRPC server is told to stop
    Then the gRPC server should stop without an error

  @category/happy
  Scenario: A call in flight finishes before the server stops
    Given the gRPC echo command takes 300 milliseconds and answers "done"
    And the gRPC server is serving on a free port
    When I start a call of the gRPC test method over TCP
    And the gRPC server is told to stop
    Then the gRPC call in flight should succeed with "done"
    And the gRPC server should stop without an error

  @category/error
  Scenario: A port that is already taken stops the server with an error
    Given the port the gRPC server wants is already taken
    When the gRPC server starts
    Then the gRPC server should stop with an error

  @category/negative
  Scenario: An unusable setting stops the server before it listens
    When the gRPC server starts with the port "grpc"
    Then the gRPC server should stop with an error mentioning "GRPC_LISTEN_PORT"
