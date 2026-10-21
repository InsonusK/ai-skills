@type/tech-check
Feature: Base log handler
  As an operator of a service without the logging variation point
  I want every record written as one JSON line without the data of the call
  So that I see who called what and how it ended, and no secret of a payload ever reaches the log

  @category/happy
  Scenario: A record keeps its attributes and loses its payload
    When a record "command called" is logged with the command "PlanItems" and the payload "password=s3cret"
    Then the line should have "msg" = "command called"
    And the line should have "command" = "PlanItems"
    And the line should have no "payload"
    And the line should not mention "s3cret"

  @category/boundary
  Scenario: A record below the level is not written
    Given the level is "WARN"
    When a record "command called" is logged with the command "PlanItems" and the payload "x"
    Then nothing should be written

  @category/happy
  Scenario: A request id put into the context is read back
    When the request id "abc123" is put into the context
    Then the request id of the context should be "abc123"

  @category/boundary
  Scenario: An empty request id is replaced by a new random one
    When the request id "" is put into the context
    Then the request id of the context should be 16 hex characters
