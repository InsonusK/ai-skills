@type/tech-check
Feature: Masking the payload
  As an operator who reads the payloads of calls in the log
  I want every secret masked, whether its field was marked or not
  So that the log never holds a password, a token, or a field the code asked to hide

  @category/happy
  Scenario: A field tagged log:"mask" is masked and one tagged log:"omit" is left out
    When the payload is a command with the name "Ann", the pin "1234" tagged mask, and the note "x" tagged omit
    Then the masked payload should be '{"name":"Ann","pin":"***"}'

  @category/happy
  Scenario Outline: A field named like a secret is masked though nobody tagged it
    When the payload is a map with "<key>" = "s3cret" and "title" = "buy milk"
    Then the masked payload should mention "buy milk"
    And the masked payload should not mention "s3cret"

    Examples:
      | key           |
      | password      |
      | access_token  |
      | Authorization |
      | api-key       |
      | client_secret |

  @category/happy
  Scenario: A JSON body is parsed and masked by its keys
    When the payload is the body '{"user":"ann","password":"s3cret","items":[{"token":"t"}]}'
    Then the masked payload should be '{"items":[{"token":"***"}],"password":"***","user":"ann"}'

  @category/boundary
  Scenario: A body that is not JSON is written as text
    When the payload is the body 'plain text'
    Then the masked payload should be '"plain text"'

  @category/happy
  Scenario: A field of a proto message with debug_redact is masked
    When the payload is a proto message with the title "buy milk" and the pin "1234" marked debug_redact
    Then the masked payload should be '{"pin":"***","title":"buy milk"}'
