@type/tech-check
Feature: Payloads only Go gives
  As an operator of a Go service with the logging variation point
  I want a payload given to logger.With masked like one given to a record, and Go values written as Go writes them
  So that no way of passing a payload in Go escapes the mask

  # The paths and values only Go has (H06 of leak-catalog.md); the shared
  # scenarios are masking.feature and payload-policy.feature.

  @category/happy
  Scenario: [H06] A payload given to logger.With is masked, and its unparsed body warned with the first record
    When the payload password "s3cret" and the body 'a=b' are bound with logger.With, and 2 records "command called" are logged
    Then the log should not mention "s3cret"
    And the log should hold these warnings:
      | record         | field  | bytes |
      | command called | $.body | 3     |

  @category/happy
  Scenario: A value that writes itself as text, like a time, is written as that text
    When the payload is the time "2026-10-22T10:00:00Z"
    Then the masked payload should be '"2026-10-22T10:00:00Z"'

  @category/negative
  Scenario: A payload that cannot be written as JSON says so instead of failing the record
    When a record is logged with a payload that cannot be written as JSON
    Then the record should mention "payload not written"
