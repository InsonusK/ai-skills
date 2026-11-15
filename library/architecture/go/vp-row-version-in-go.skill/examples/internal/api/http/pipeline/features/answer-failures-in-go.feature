@type/tech-check
Feature: Answering failures, in Go
  As a client of the service
  I want a request whose handler panicked answered as google.rpc.Status, not with a cut connection
  So that a Go panic never cuts the connection, and the panic is in the log

  @category/error
  Scenario: A route that panics is answered INTERNAL and its panic is logged
    Given the echo command crashes
    When I POST "/v1/echo" to the HTTP API with the body '{"text":"hi"}'
    Then the HTTP status should be 500
    And the HTTP error should have code 13 and the message "internal error"
    And a "http request panicked" record should be logged
