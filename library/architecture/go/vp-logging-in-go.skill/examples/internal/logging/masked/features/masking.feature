# Shared by every stack's logging variation point. The source is
# library/architecture/spec/vp-logging.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited. Each scenario names the
# leaks of leak-catalog.md it closes.
@type/tech-check
Feature: Masking the payload
  As an operator who reads the payloads of calls in the log
  I want every secret masked, whether its field was marked or not, and every body the mask cannot read kept out
  So that the log never holds a password, a token, or a field the code asked to hide

  # The types the scenarios give as a payload, defined by each stack's steps:
  #   Person { name; pin marked to mask; note marked to omit; draft left out of its JSON form }
  #   Team   { title; lead: Person; members: list of Person; vault: Vault marked to mask }
  #   Vault  { code }
  #   Login  { user; password }    nothing marked
  #   Link   { name; next: Link }  may refer to itself
  #   Pin, Wallet, Node            the proto messages of masking.proto
  # Team.lead given with a note shows the note left out: a Person's note is marked to omit.
  # "N levels deep" is N maps, lists, or Node messages, the innermost holding
  # the text "end": the text is N levels below the payload.
  # A "body" is raw bytes, as an entry point reads them; a "text" is a string.
  #
  # A payload is given as JSON in a doc string: the fields of the type it is
  # built into, a body as its bytes, a map as its entries, a proto message in
  # proto JSON. "// ..." after a value says what is special about the field -
  # how it is marked, why it is masked; the steps drop it before parsing. The
  # masked payload is compared as JSON, whatever the order of its keys.

  @category/happy
  Scenario: [L01 L02 L03] A field marked to mask is masked, one marked to omit or left out of its JSON form is not written
    When the payload is a Person:
      """jsonc
      {
        "name": "Ann",
        "pin": "1234",     // marked to mask
        "note": "x",       // marked to omit
        "draft": "d"       // left out of the type's JSON form
      }
      """
    Then the masked payload should be:
      """json
      {
        "name": "Ann",
        "pin": "***"
      }
      """

  @category/happy
  Scenario: [L01 L06] Marks hold at every depth, and a marked object is masked whole
    When the payload is a Team:
      """jsonc
      {
        "title": "core",
        "lead": { "name": "Ann", "pin": "1234" },          // a Person: its pin is marked to mask
        "members": [ { "name": "Bob", "pin": "5678" } ],   // Persons in a list
        "vault": { "code": "9999" }                        // marked to mask: the whole object
      }
      """
    Then the masked payload should be:
      """json
      {
        "title": "core",
        "lead": { "name": "Ann", "pin": "***" },
        "members": [ { "name": "Bob", "pin": "***" } ],
        "vault": "***"
      }
      """

  @category/happy
  Scenario: [L04] A field named like a secret is masked though nobody marked it
    When the payload is a Login:
      """jsonc
      {
        "user": "ann",
        "password": "s3cret"   // not marked: masked by its name
      }
      """
    Then the masked payload should be:
      """json
      {
        "user": "ann",
        "password": "***"
      }
      """

  @category/happy
  Scenario Outline: [L04] A key named like a secret is masked whatever its case, separators, and affixes
    When the payload is a map with "<key>" = "s3cret" and "title" = "buy milk"
    Then the masked payload should mention "buy milk"
    And the masked payload should not mention "s3cret"

    Examples:
      | key                 |
      | password            |
      | db_password         |
      | Passwd              |
      | pwd                 |
      | passphrase          |
      | secret              |
      | client_secret       |
      | token               |
      | access_token        |
      | refreshToken        |
      | Authorization       |
      | Proxy-Authorization |
      | Cookie              |
      | Set-Cookie          |
      | api-key             |
      | X-Api-Key           |
      | apiKey              |
      | private_key         |
      | credentials         |
      | session_id          |
      | cvv                 |

  @category/happy
  Scenario: [L05] A key named like a secret is masked whole when it holds an object
    When the payload is the body:
      """jsonc
      {
        "user": "ann",
        "credentials": { "name": "ann", "value": "s3cret" }   // a secret name: the whole object
      }
      """
    Then the masked payload should be:
      """json
      {
        "user": "ann",
        "credentials": "***"
      }
      """

  @category/happy
  Scenario: [L15] A header named like a secret is masked with all its values
    When the payload is a map of lists:
      """jsonc
      {
        "Authorization": [ "Bearer s3cret" ],   // a secret name: every value
        "Accept": [ "text/plain" ]
      }
      """
    Then the masked payload should be:
      """json
      {
        "Authorization": "***",
        "Accept": [ "text/plain" ]
      }
      """

  @category/happy
  Scenario: [L07] A JSON body is parsed and masked by its keys at every depth
    When the payload is the body:
      """jsonc
      {
        "user": "ann",
        "password": "s3cret",                               // a secret name
        "items": [ { "token": "t", "title": "buy milk" } ]  // a secret name in a list
      }
      """
    Then the masked payload should be:
      """json
      {
        "user": "ann",
        "password": "***",
        "items": [ { "token": "***", "title": "buy milk" } ]
      }
      """
    And the mask should report no unparsed body

  @category/happy
  Scenario: [L08] A text holding a JSON object is parsed and masked by its keys
    When the payload is a map:
      """jsonc
      {
        "meta": "{\"token\":\"t\",\"size\":1}"   // a text that holds JSON
      }
      """
    Then the masked payload should be:
      """json
      {
        "meta": { "token": "***", "size": 1 }
      }
      """

  @category/boundary
  Scenario: [L08] A text that is not JSON stays as it is, though it starts like JSON
    When the payload is a map:
      """jsonc
      {
        "title": "[urgent] buy milk"   // starts like a JSON list, is not one
      }
      """
    Then the masked payload should be:
      """json
      {
        "title": "[urgent] buy milk"
      }
      """
    And the mask should report no unparsed body

  @category/negative
  Scenario: [L09] A body that is not JSON is written as unparsed, never as it is
    When the payload is the body 'user=ann&password=s3cret'
    Then the masked payload should be '"<unparsed body, 24 bytes>"'
    And the masked payload should not mention "s3cret"
    And the mask should report the unparsed body "$" of 24 bytes

  @category/negative
  Scenario: [L09] An unparsed body inside a payload is reported by its path
    When the payload is a map with "body" holding the body 'user=ann&password=s3cret'
    Then the masked payload should be '{"body":"<unparsed body, 24 bytes>"}'
    And the mask should report the unparsed body "$.body" of 24 bytes

  @category/boundary
  Scenario: [L10] An empty body is an empty text, not an unparsed one
    When the payload is the body ''
    Then the masked payload should be '""'
    And the mask should report no unparsed body

  @category/happy
  Scenario: [L11] A proto field marked debug_redact is masked
    When the payload is a Pin message:
      """jsonc
      {
        "title": "buy milk",
        "pin": "1234"          // [debug_redact = true]
      }
      """
    Then the masked payload should be:
      """json
      {
        "title": "buy milk",
        "pin": "***"
      }
      """

  @category/happy
  Scenario: [L11 L12] debug_redact holds in nested and repeated messages; a map key and a field named like a secret are masked
    When the payload is a Wallet message:
      """jsonc
      {
        "owner": "ann",
        "main": { "title": "a", "pin": "1" },             // a Pin: pin is debug_redact
        "spare": [ { "title": "b", "pin": "2" } ],        // Pins in a repeated field
        "labels": { "token": "t", "color": "red" },       // a map: token is a secret name
        "apiKey": "k",                                     // api_key: a secret name
        "kind": "KIND_GOLD"
      }
      """
    Then the masked payload should be:
      """json
      {
        "owner": "ann",
        "main": { "title": "a", "pin": "***" },
        "spare": [ { "title": "b", "pin": "***" } ],
        "labels": { "token": "***", "color": "red" },
        "apiKey": "***",
        "kind": "KIND_GOLD"
      }
      """

  @category/boundary
  Scenario: [L11] An enum value the message does not know is written as its number
    When the payload is a Wallet message:
      """jsonc
      {
        "owner": "ann",
        "kind": 7              // no value of Kind has the number 7
      }
      """
    Then the masked payload should be:
      """json
      {
        "owner": "ann",
        "kind": 7
      }
      """

  @category/negative
  Scenario: [L13] A proto bytes field that is not JSON is written as unparsed
    When the payload is a Wallet message:
      """jsonc
      {
        "owner": "ann",
        "blob": "YWJj"         // bytes in proto JSON: base64 of "abc", which is not JSON
      }
      """
    Then the masked payload should be:
      """json
      {
        "owner": "ann",
        "blob": "<unparsed body, 3 bytes>"
      }
      """
    And the mask should report the unparsed body "$.blob" of 3 bytes

  @category/happy
  Scenario: [L14] An Any of a known type is unpacked and masked like its message
    When the payload is a Wallet message:
      """jsonc
      {
        "extra": {             // an Any holding a Pin
          "@type": "type.googleapis.com/conformance.masking.Pin",
          "title": "t",
          "pin": "1234"
        }
      }
      """
    Then the masked payload should be:
      """json
      {
        "extra": {
          "@type": "type.googleapis.com/conformance.masking.Pin",
          "title": "t",
          "pin": "***"
        }
      }
      """

  @category/negative
  Scenario: [L14] An Any of an unknown type is written as unparsed
    When the payload is a Wallet message whose extra is an Any of the type "type.googleapis.com/unknown.Thing" holding 5 bytes
    Then the masked payload should be '{"extra":"<unparsed body, 5 bytes>"}'
    And the mask should report the unparsed body "$.extra" of 5 bytes

  # With LOG_PAYLOAD_UNMASKED, for debugging only, nothing is masked: the
  # payload is written as the data it is. What is not data of the call stays
  # out - a field marked to omit, a field left out of the type's JSON form -
  # and the depth limit still holds.

  @category/security
  Scenario: [U02] With masking off, marked fields and secret names are written as they are
    Given masking is switched off
    When the payload is a Team:
      """jsonc
      {
        "title": "core",
        "lead": { "name": "Ann", "pin": "1234", "note": "x" },   // pin marked to mask, note marked to omit
        "members": [],
        "vault": { "code": "9999" }                               // marked to mask
      }
      """
    Then the payload should be written as:
      """json
      {
        "title": "core",
        "lead": { "name": "Ann", "pin": "1234" },
        "members": [],
        "vault": { "code": "9999" }
      }
      """

  @category/security
  Scenario: [U02] With masking off, a body is written as it came, JSON or not
    Given masking is switched off
    When the payload is a map with "body" holding the body 'user=ann&password=s3cret'
    Then the payload should be written as:
      """json
      { "body": "user=ann&password=s3cret" }
      """
    And the mask should report no unparsed body

  @category/security
  Scenario: [U02] With masking off, debug_redact and secret names of a proto message are written as they are
    Given masking is switched off
    When the payload is a Wallet message:
      """jsonc
      {
        "main": { "title": "a", "pin": "1" },   // pin is debug_redact
        "apiKey": "k",                           // a secret name
        "blob": "YWJj"                           // base64 of "abc", not JSON
      }
      """
    Then the payload should be written as:
      """json
      {
        "main": { "title": "a", "pin": "1" },
        "apiKey": "k",
        "blob": "abc"
      }
      """

  @category/boundary
  Scenario Outline: [F01] A value deeper than 32 levels is written as too deep, so a cycle cannot overflow the stack
    When the payload is <shape> nested <levels> levels deep
    Then the masked payload should <written> "<too deep>"

    Examples:
      | shape                                 | levels | written     |
      | maps                                  | 32     | not mention |
      | maps                                  | 33     | mention     |
      | lists                                 | 32     | not mention |
      | lists                                 | 33     | mention     |
      | Node messages linked by next          | 32     | not mention |
      | Node messages linked by next          | 33     | mention     |
      | Node messages each in children        | 40     | mention     |
      | Node messages each in named           | 40     | mention     |

  @category/boundary
  Scenario: [F01] A payload that refers to itself is written down to the limit, not forever
    When the payload is a link "a" whose next is itself
    Then the masked payload should mention "<too deep>"
