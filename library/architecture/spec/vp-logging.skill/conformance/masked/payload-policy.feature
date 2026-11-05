# Shared by every stack's logging variation point. The source is
# library/architecture/spec/vp-logging.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited. Each scenario names the
# leaks of leak-catalog.md it closes.
@type/tech-check
Feature: Writing the payload of a record
  As an operator of a service with the logging variation point
  I want the payload of each call written masked, with the request id, within a size, and an unreadable body flagged once
  So that I can see what a failed call received without the log leaking a secret or growing without bound

  # A record is logged at INFO with the message "command called" unless a step
  # names another, the attribute command = "PlanItems", failed, and the payload.
  # "The record" is the first line written that is not a warning of the
  # handler; "the log" is every line written.

  @category/happy
  Scenario: [H01 H02] A record keeps its payload, masked, and gets the request id of its context
    Given the request id "req-42" is in the context
    When a record is logged with the payload password "s3cret" and title "buy milk"
    Then the record should have "request_id" = "req-42"
    And the record should have "command" = "PlanItems"
    And the record should mention "buy milk"
    And the log should not mention "s3cret"

  @category/happy
  Scenario Outline: [H03] LOG_PAYLOAD decides which records keep their payload
    Given LOG_PAYLOAD is "<mode>"
    When a record with failed "<failed>" is logged with the payload title "buy milk"
    Then the record should <keep> "payload"

    Examples:
      | mode   | failed | keep     |
      | always | false  | have     |
      | always | true   | have     |
      | errors | false  | not have |
      | errors | true   | have     |
      | off    | false  | not have |
      | off    | true   | not have |

  @category/boundary
  Scenario: [H04] A payload larger than LOG_PAYLOAD_MAX_BYTES is cut and says how long it was
    Given LOG_PAYLOAD_MAX_BYTES is 64
    When a record is logged with the payload password "s3cret" and a title of 200 characters
    Then the record should mention "(cut, "
    And the log should not mention "s3cret"

  @category/boundary
  Scenario Outline: [H04] A payload is cut once its JSON is longer than LOG_PAYLOAD_MAX_BYTES, not before
    Given LOG_PAYLOAD_MAX_BYTES is 64
    When a record is logged with the payload title of <length> characters
    Then the record should <cut> "(cut, <bytes> bytes)"

    # {"title":"..."} is 12 bytes and the title.
    Examples:
      | length | bytes | cut         |
      | 52     | 64    | not mention |
      | 53     | 65    | mention     |

  @category/negative
  Scenario: [H05] An unparsed body keeps its record's level and is warned once per record and field
    Given the request id "req-42" is in the context
    When a record "http request received" is logged with the payload body 'user=ann&password=s3cret'
    And a record "http request received" is logged with the payload body 'user=bob&password=hunter2'
    And a record "http response sent" is logged with the payload body 'user=ann&password=s3cret'
    Then every record should have "level" = "INFO"
    And the log should not mention "s3cret"
    And the log should not mention "hunter2"
    And the log should hold these warnings:
      | record                | field  | bytes |
      | http request received | $.body | 24    |
      | http response sent    | $.body | 24    |
    And every warning should have "request_id" = "req-42"

  @category/boundary
  Scenario: [H05] A warning below the level the log is written at is not written
    Given the log is written from the level "ERROR"
    When a record "command failed" is logged at ERROR with the payload body 'user=ann&password=s3cret'
    Then the record should mention "<unparsed body, 24 bytes>"
    And the log should hold no warning

  @category/boundary
  Scenario: [H05] A record that does not keep its payload warns about nothing
    Given LOG_PAYLOAD is "off"
    When a record "http request received" is logged with the payload body 'user=ann&password=s3cret'
    Then the log should hold no warning

  @category/security
  Scenario: [U01] Masking switched off is warned at start, then once per request
    Given masking is switched off
    When a record is logged with the payload password "s3cret" and title "buy milk" in the request "req-1"
    And a record is logged with the payload password "s3cret" and title "buy milk" in the request "req-1"
    And a record is logged with the payload password "s3cret" and title "buy milk" in the request "req-2"
    Then the record should mention "s3cret"
    And the log should hold these "log payload unmasked" warnings:
      | request_id |
      |            |
      | req-1      |
      | req-2      |

  @category/security
  Scenario: [U01] With masking off, a record outside a request is warned every time
    Given masking is switched off
    When a record is logged with the payload password "s3cret" and title "buy milk"
    And a record is logged with the payload password "s3cret" and title "buy milk"
    Then the log should hold these "log payload unmasked" warnings:
      | request_id |
      |            |
      |            |
      |            |

  @category/security
  Scenario: [U01] With masking off, a body that is not JSON is written as it came and not warned as unparsed
    Given masking is switched off
    When a record "http request received" is logged with the payload body 'user=ann&password=s3cret'
    Then the record should mention "user=ann&password=s3cret"
    And the log should hold no "log payload not parsed" warning

  @category/boundary
  Scenario: [U01] With masking on, nothing is warned about unmasking
    When a record is logged with the payload password "s3cret" and title "buy milk"
    Then the log should hold these "log payload unmasked" warnings:
      | request_id |
