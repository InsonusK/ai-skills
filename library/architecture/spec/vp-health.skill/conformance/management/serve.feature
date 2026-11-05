@type/tech-check
Feature: Management server lifecycle
  As the orchestrator that stops the service
  I want the management server to stop cleanly when the process is told to terminate
  So that a rolling update does not report a failure

  @category/happy
  Scenario: The server stops without an error when it is told to stop
    Given the management server is serving on a free port
    When the management server is told to stop
    Then the management server should stop without an error

  @category/error
  Scenario: A port that is already taken stops the server with an error
    Given the port the management server wants is already taken
    When the management server starts
    Then the management server should stop with an error

  @category/negative
  Scenario: An unusable setting stops the server before it listens
    When the management server starts with the port "mgmt"
    Then the management server should stop with an error mentioning "MANAGEMENT_LISTEN_PORT"
