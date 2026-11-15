@type/tech-check
Feature: Base log handler
  As an operator of a service without the logging variation point
  I want every record written as one JSON line without the data of the call
  So that I see who called what and how it ended, and no secret of a payload ever reaches the log

  @category/happy
  Scenario: A record keeps its attributes and loses its payload
    When a record "command called" is logged with the command "PlanItems" and the payload "password=s3cret"
    Then the line should have "msg" = "command called"
    And the line should have "level" = "INFO"
    And the line should have "command" = "PlanItems"
    And the line should have no "payload"
    And the line should not mention "s3cret"

  @category/boundary
  Scenario: A record below the level is not written
    Given the level is "WARN"
    When a record "command called" is logged with the command "PlanItems" and the payload "x"
    Then nothing should be written

  @category/happy
  Scenario: A duration is written in nanoseconds, as every stack writes it
    When a record "command handled" is logged with a duration of 2 milliseconds
    Then the line should have "duration" = "2000000"

  @category/error
  Scenario: A rejected request is written with its message, without a stack or a cause
    When a record "command failed" is logged with an error "item titles repeat" of the kind "invalid"
    Then the line should have "error" = "item titles repeat"
    And the line should have no "exception"
    And the line should have no "cause"

  @category/error
  Scenario: A failed command with a kind logs the message it answered and, apart, its cause
    When a record "command failed" is logged with an error "the lists are unavailable" of the kind "unavailable" caused by "dial tcp 10.0.0.5:5432: connection refused"
    Then the line should have "error" = "the lists are unavailable"
    And the line should have "cause" = "dial tcp 10.0.0.5:5432: connection refused"
    And the line should have no "exception"

  @category/happy
  Scenario: A request id begun for a call is read back
    When the request id "abc123" is begun
    Then the current request id should be "abc123"

  @category/boundary
  Scenario: An empty request id is replaced by a new random one
    When the request id "" is begun
    Then the current request id should be 16 hex characters
