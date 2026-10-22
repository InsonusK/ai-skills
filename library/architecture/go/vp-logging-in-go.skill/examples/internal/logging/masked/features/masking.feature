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
  #   Account { user; password, whose JSON name is "p" }
  #   Upload { name; content: a body }
  #   Link   { name; next: Link }  may refer to itself
  #   Pin, Wallet, Node            the proto messages of masking.proto
  # "N levels deep" is N maps, lists, JSON objects of a body, or Node messages,
  # the innermost holding the text "end": the text is N levels below the payload.
  # A "body" is raw bytes, as an entry point reads them; a "text" is a string.

  @category/happy
  Scenario: [L01 L02 L03] A field marked to mask is masked, one marked to omit or left out of its JSON form is not written
    When the payload is a person with the name "Ann", the pin "1234", the note "x", and the draft "d"
    Then the masked payload should be '{"name":"Ann","pin":"***"}'

  @category/happy
  Scenario: [L01 L06] Marks hold at every depth, and a marked object is masked whole
    When the payload is a team "core" led by "Ann" with the pin "1234", a member "Bob" with the pin "5678", and the vault code "9999"
    Then the masked payload should be '{"title":"core","lead":{"name":"Ann","pin":"***"},"members":[{"name":"Bob","pin":"***"}],"vault":"***"}'

  @category/happy
  Scenario: [L04] A field named like a secret is masked though nobody marked it
    When the payload is a login with the user "ann" and the password "s3cret"
    Then the masked payload should be '{"user":"ann","password":"***"}'

  @category/happy
  Scenario: [L04] A field named like a secret is masked though its JSON name is not
    When the payload is an account with the user "ann" and the password "s3cret"
    Then the masked payload should be '{"user":"ann","p":"***"}'

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
    When the payload is the body '{"user":"ann","credentials":{"name":"ann","value":"s3cret"}}'
    Then the masked payload should be '{"user":"ann","credentials":"***"}'

  @category/happy
  Scenario: [L15] A header named like a secret is masked with all its values
    When the payload is a map of lists with "Authorization" = "Bearer s3cret" and "Accept" = "text/plain"
    Then the masked payload should be '{"Accept":["text/plain"],"Authorization":"***"}'

  @category/boundary
  Scenario: [L06] A value that is not set is written as null
    When the payload is a map with "note" holding nothing
    Then the masked payload should be '{"note":null}'

  @category/happy
  Scenario: [L07] A JSON body is parsed and masked by its keys at every depth
    When the payload is the body '{"user":"ann","password":"s3cret","note":null,"items":[{"token":"t","title":"buy milk"}]}'
    Then the masked payload should be '{"user":"ann","password":"***","note":null,"items":[{"token":"***","title":"buy milk"}]}'
    And the mask should report no unparsed body

  @category/happy
  Scenario: [L08] A text holding a JSON object is parsed and masked by its keys
    When the payload is a map with "meta" holding the text '{"token":"t","size":1}'
    Then the masked payload should be '{"meta":{"token":"***","size":1}}'

  @category/happy
  Scenario: [L08] A text holding JSON inside a JSON body is parsed and masked as well
    When the payload is the body '{"meta":"{\"token\":\"t\"}"}'
    Then the masked payload should be '{"meta":{"token":"***"}}'

  @category/boundary
  Scenario: [L08] A text that is not JSON stays as it is, though it starts like JSON
    When the payload is a map with "title" holding the text '[urgent] buy milk'
    Then the masked payload should be '{"title":"[urgent] buy milk"}'
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

  @category/negative
  Scenario: [L09] An unparsed body in a field of an object is reported by the field's JSON name
    When the payload is an upload "a.txt" with the content 'a=b'
    Then the masked payload should be '{"name":"a.txt","content":"<unparsed body, 3 bytes>"}'
    And the mask should report the unparsed body "$.content" of 3 bytes

  @category/negative
  Scenario: [L09] Unparsed bodies in a list are reported by their index
    When the payload is a map with "parts" holding the bodies 'a=1', '{"x":1}', and 'b=22'
    Then the masked payload should be '{"parts":["<unparsed body, 3 bytes>",{"x":1},"<unparsed body, 4 bytes>"]}'
    And the mask should report these unparsed bodies:
      | path        | bytes |
      | $.parts[0]  | 3     |
      | $.parts[2]  | 4     |

  @category/boundary
  Scenario: [L10] An empty body is an empty text, not an unparsed one
    When the payload is the body ''
    Then the masked payload should be '""'
    And the mask should report no unparsed body

  @category/happy
  Scenario: [L11] A proto field marked debug_redact is masked
    When the payload is a Pin message given as '{"title":"buy milk","pin":"1234"}'
    Then the masked payload should be '{"title":"buy milk","pin":"***"}'

  @category/happy
  Scenario: [L11 L12] debug_redact holds in nested and repeated messages; a map key and a field named like a secret are masked
    When the payload is a Wallet message given as '{"owner":"ann","main":{"title":"a","pin":"1"},"spare":[{"title":"b","pin":"2"}],"labels":{"token":"t","color":"red"},"apiKey":"k","kind":"KIND_GOLD"}'
    Then the masked payload should be '{"owner":"ann","main":{"title":"a","pin":"***"},"spare":[{"title":"b","pin":"***"}],"labels":{"token":"***","color":"red"},"apiKey":"***","kind":"KIND_GOLD"}'

  @category/boundary
  Scenario Outline: [L11] A proto field is written as proto JSON writes it: when set, not at its default
    When the payload is a Wallet message given as '<given>'
    Then the masked payload should be '<written>'

    Examples:
      | given                                  | written                     |
      | {"owner":"ann","count":2}              | {"owner":"ann","count":2}   |
      | {"owner":"ann","count":0,"kind":0}     | {"owner":"ann"}             |
      | {"owner":"","blobs":[],"files":{}}     | {}                          |

  @category/boundary
  Scenario: [L11] An enum value the message does not know is written as its number
    When the payload is a Wallet message given as '{"owner":"ann","kind":7}'
    Then the masked payload should be '{"owner":"ann","kind":7}'

  @category/negative
  Scenario: [L13] A proto bytes field that is not JSON is written as unparsed
    When the payload is a Wallet message given as '{"owner":"ann","blob":"YWJj"}'
    Then the masked payload should be '{"owner":"ann","blob":"<unparsed body, 3 bytes>"}'
    And the mask should report the unparsed body "$.blob" of 3 bytes

  @category/negative
  Scenario: [L13] Proto bytes in a repeated field and a map are reported by their path
    When the payload is a Wallet message given as '{"blobs":["e30=","YWJj"],"files":{"a":"YWJj"}}'
    Then the masked payload should be '{"blobs":[{},"<unparsed body, 3 bytes>"],"files":{"a":"<unparsed body, 3 bytes>"}}'
    And the mask should report these unparsed bodies:
      | path       | bytes |
      | $.blobs[1] | 3     |
      | $.files.a  | 3     |

  @category/happy
  Scenario: [L14] An Any of a known type is unpacked and masked like its message
    When the payload is a Wallet message given as '{"extra":{"@type":"type.googleapis.com/conformance.masking.Pin","title":"t","pin":"1234"}}'
    Then the masked payload should be '{"extra":{"@type":"type.googleapis.com/conformance.masking.Pin","title":"t","pin":"***"}}'

  @category/negative
  Scenario: [L14] An Any of an unknown type is written as unparsed
    When the payload is a Wallet message whose extra is an Any of the type "type.googleapis.com/unknown.Thing" holding 5 bytes
    Then the masked payload should be '{"extra":"<unparsed body, 5 bytes>"}'
    And the mask should report the unparsed body "$.extra" of 5 bytes

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
      | a JSON body of objects                | 32     | not mention |
      | a JSON body of objects                | 33     | mention     |
      | a JSON body of lists                  | 32     | not mention |
      | a JSON body of lists                  | 33     | mention     |
      | Node messages linked by next          | 32     | not mention |
      | Node messages linked by next          | 33     | mention     |
      | Node messages each in children        | 40     | mention     |
      | Node messages each in named           | 40     | mention     |

  @category/boundary
  Scenario: [F01] A payload that refers to itself is written down to the limit, not forever
    When the payload is a link "a" whose next is itself
    Then the masked payload should mention "<too deep>"
