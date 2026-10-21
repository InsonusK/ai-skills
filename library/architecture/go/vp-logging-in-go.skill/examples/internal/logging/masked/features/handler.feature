@type/tech-check
Feature: Masked log handler
  As an operator of a service with the logging variation point
  I want the payload of each call written masked, with the request id, within a size
  So that I can see what a failed call received without the log growing without bound

  @category/happy
  Scenario: A record keeps its payload, masked, and gets the request id of its context
    Given the request id "req-42" is in the context
    When a record is logged with the payload password "s3cret" and title "buy milk"
    Then the line should have "request_id" = "req-42"
    And the line should mention "buy milk"
    And the line should not mention "s3cret"

  @category/happy
  Scenario Outline: LOG_PAYLOAD decides which records keep their payload
    Given LOG_PAYLOAD is "<mode>"
    When a record with failed "<failed>" is logged with the payload title "buy milk"
    Then the line should <keep> "payload"

    Examples:
      | mode   | failed | keep     |
      | always | false  | have     |
      | always | true   | have     |
      | errors | false  | not have |
      | errors | true   | have     |
      | off    | true   | not have |

  @category/boundary
  Scenario: A payload larger than LOG_PAYLOAD_MAX_BYTES is cut and says how long it was
    Given LOG_PAYLOAD_MAX_BYTES is 64
    When a record is logged with a payload of 200 characters
    Then the line should mention "(cut, "
