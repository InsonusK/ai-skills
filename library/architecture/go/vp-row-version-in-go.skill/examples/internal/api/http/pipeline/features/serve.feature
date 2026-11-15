@type/tech-check
Feature: HTTP server lifecycle
  As the orchestrator that stops the service
  I want the HTTP server to stop cleanly when the process is told to terminate
  So that a rolling update does not cut requests off or report a failure

  @category/happy
  Scenario: The server stops without an error when it is told to stop
    Given the HTTP server is serving on a free port
    When the HTTP server is told to stop
    Then the HTTP server should stop without an error

  @category/error
  Scenario: A port that is already taken stops the server with an error
    Given the port the HTTP server wants is already taken
    When the HTTP server starts
    Then the HTTP server should stop with an error

  @category/negative
  Scenario: An unusable setting stops the server before it listens
    When the HTTP server starts with the port "http"
    Then the HTTP server should stop with an error mentioning "HTTP_LISTEN_PORT"
