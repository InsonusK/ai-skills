@type/tech-check
Feature: Server lifecycle
  As the orchestrator that stops the service
  I want the server to stop cleanly when the process is told to terminate
  So that a rolling update does not cut requests off or report a failure

  @category/happy
  Scenario: The server stops without an error when its context is cancelled
    Given the server is serving on a free port
    When the process is told to terminate
    Then the server should stop without an error

  @category/error
  Scenario: A port that is already taken stops the server with an error
    Given the port the server wants is already taken
    When the server starts
    Then the server should stop with an error
