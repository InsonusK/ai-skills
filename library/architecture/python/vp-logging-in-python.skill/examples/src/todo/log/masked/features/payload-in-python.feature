@type/tech-check
Feature: Payloads only Python gives
  As an operator of a Python service with the logging variation point
  I want every way Python hands a value to a record masked, and Python values written as Python writes them
  So that no way of passing data in Python escapes the mask

  # The paths and types only Python has (H06 of leak-catalog.md), and the
  # cases the mutation run of the Python mask asked for; the shared scenarios
  # are masking.feature, payload-policy.feature, and settings.feature.

  @category/security
  Scenario: [H06] A message given arguments is written as written, never with them
    When a record "login of %s" is logged with the argument "s3cret"
    Then the record should mention "login of %s"
    And the log should not mention "s3cret"

  @category/happy
  Scenario: [H06] A payload given through a LoggerAdapter is masked like one given to the record
    When a LoggerAdapter with the payload password "s3cret" and title "buy milk" logs a record
    Then the record should mention "buy milk"
    And the log should not mention "s3cret"

  @category/happy
  Scenario: [L01 L02 L04] A dataclass is masked by the marks of its Annotated fields and by the secret names
    When the payload is a dataclass with the pin "1234" marked to mask, the note "x" marked to omit, the token "t", and the title "buy milk"
    Then the masked payload should be '{"pin":"***","token":"***","title":"buy milk"}'

  @category/happy
  Scenario: [L01] Any other object is written by its public attributes, masked by the marks of its class
    When the payload is an object with the title "buy milk", the pin "1234" marked to mask, and the private cache "c"
    Then the masked payload should be '{"title":"buy milk","pin":"***"}'

  @category/negative
  Scenario Outline: [L09] A body in a field of a dataclass or an object is reported by the field's name
    When the payload is <holder> with the name "a.txt" and the content 'a=b'
    Then the masked payload should be '{"name":"a.txt","content":"<unparsed body, 3 bytes>"}'
    And the mask should report the unparsed body "$.content" of 3 bytes

    Examples:
      | holder      |
      | a dataclass |
      | an object   |

  @category/happy
  Scenario Outline: A value that writes itself as text is written as that text
    When the payload is the <value>
    Then the masked payload should be '<written>'

    Examples:
      | value                                   | written                     |
      | time "2026-10-22T10:00:00+00:00"        | "2026-10-22T10:00:00+00:00" |
      | enum member KIND_GOLD = "gold"          | "gold"                      |
      | decimal "12.50"                         | "12.50"                     |
      | object of no attributes written "draft" | "draft"                     |

  @category/happy
  Scenario: [L08] A text holding a JSON array is parsed and masked by its keys
    When the payload is a map with "meta" = '[{"token":"t","size":1}]'
    Then the masked payload should be '{"meta":[{"token":"***","size":1}]}'

  @category/happy
  Scenario: [L08 L11] A proto string holding JSON is parsed and masked by its keys
    When the payload is a Pin message given as '{"title":"{\"token\":\"t\"}"}'
    Then the masked payload should be '{"title":{"token":"***"}}'

  @category/happy
  Scenario: [L14] An Any is unpacked by the last part of its type URL, and a body in it reported by its path
    When the payload is a Wallet whose extra is an Any of the type "example.com/types/conformance.masking.Wallet" holding a Wallet with the blob 'abc'
    Then the masked payload should be '{"extra":{"@type":"example.com/types/conformance.masking.Wallet","blob":"<unparsed body, 3 bytes>"}}'
    And the mask should report the unparsed body "$.extra.blob" of 3 bytes

  @category/boundary
  Scenario Outline: [F01] The depth limit is the same for dataclasses, proto lists and maps, and with masking off
    When the payload is <shape> nested <levels> levels deep
    Then the masked payload should <written> "<too deep>"

    Examples:
      | shape                                   | levels | written     |
      | dataclass links                         | 32     | not mention |
      | dataclass links                         | 33     | mention     |
      | Node messages each in children          | 16     | not mention |
      | Node messages each in children          | 17     | mention     |
      | Node messages each in named             | 16     | not mention |
      | Node messages each in named             | 17     | mention     |
      | maps, masking off,                      | 32     | not mention |
      | maps, masking off,                      | 33     | mention     |

  @category/security
  Scenario: [U02] With masking off, a body that is not UTF-8 text is written with its unreadable bytes replaced
    When masking is off and the payload is the bytes "ff 61"
    Then the masked payload should be '"�a"'

  @category/boundary
  Scenario: [H05] An unparsed body is warned about when the log is written from WARN
    Given the log is written from the level "WARN"
    When a record "command failed" is logged at WARN with the payload body 'a=b'
    Then the log should hold the warning "log payload not parsed"

  @category/boundary
  Scenario Outline: [H04] A payload is measured in UTF-8 bytes, and a character cut in two is dropped
    When a record is logged with the payload n = 12 and a title of <count> "ж" with LOG_PAYLOAD_MAX_BYTES 64
    Then the record should <cut> "(cut, <bytes> bytes)"
    And the record should mention "<begins>"

    # {"n":12,"title":"..."} is 19 bytes and 2 bytes for each "ж"; cut at 64,
    # 24 of them end the head in the middle of a "ж".
    Examples:
      | count | bytes | cut         | begins |
      | 22    | 63    | not mention | жжжжж  |
      | 24    | 67    | mention     | жжжжж  |
