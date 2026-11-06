# Shared by every stack's logging variation point. The source is
# library/architecture/spec/vp-logging.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/tech-check
Feature: Logging settings
  As the operator who deploys the service
  I want the payload rules read from their own environment variables
  So that a busy service logs only the payloads of failed calls without a rebuild

  @category/boundary
  Scenario: [S01] Nothing set means every payload, cut at 4096 bytes, masked
    When the logging settings are loaded
    Then LOG_PAYLOAD should be "always" and the limit 4096
    And masking should be on

  @category/happy
  Scenario Outline: [S01] A usable value is taken
    Given the environment variable "LOG_PAYLOAD" is "<mode>"
    And the environment variable "LOG_PAYLOAD_MAX_BYTES" is "<limit>"
    When the logging settings are loaded
    Then LOG_PAYLOAD should be "<mode>" and the limit <limit>

    Examples:
      | mode   | limit |
      | errors | 64    |
      | off    | 10000 |

  @category/negative
  Scenario Outline: [S02] An unusable value is rejected, naming its variable
    Given the environment variable "<name>" is "<value>"
    When the logging settings are loaded
    Then loading should fail mentioning "<name>"

    Examples:
      | name                  | value  |
      | LOG_PAYLOAD           | some   |
      | LOG_PAYLOAD           | ALWAYS |
      | LOG_PAYLOAD_MAX_BYTES | ten    |
      | LOG_PAYLOAD_MAX_BYTES | 63     |
      | LOG_PAYLOAD_MAX_BYTES | -1     |

  @category/security
  Scenario: [U01] Masking is switched off only together with the debug level
    Given the environment variable "LOG_PAYLOAD_UNMASKED" is "true"
    When the logging settings are loaded for the level "DEBUG"
    Then masking should be off

  @category/security
  Scenario Outline: [U01] Masking switched off at another level stops the start, naming both variables
    Given the environment variable "LOG_PAYLOAD_UNMASKED" is "true"
    When the logging settings are loaded for the level "<level>"
    Then loading should fail mentioning "LOG_PAYLOAD_UNMASKED"
    And loading should fail mentioning "LOG_LEVEL"

    Examples:
      | level |
      | INFO  |
      | WARN  |
      | ERROR |

  @category/negative
  Scenario Outline: [U01] LOG_PAYLOAD_UNMASKED is true or false
    Given the environment variable "LOG_PAYLOAD_UNMASKED" is "<value>"
    When the logging settings are loaded for the level "DEBUG"
    Then <outcome>

    Examples:
      | value | outcome                                             |
      | false | masking should be on                                |
      | yes   | loading should fail mentioning "LOG_PAYLOAD_UNMASKED" |
